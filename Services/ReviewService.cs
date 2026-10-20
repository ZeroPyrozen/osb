#nullable enable

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.ViewModels;

namespace osb.Services;

public enum ReviewOutcome { Approved, Declined, Saved, Removed, Invalid, AlreadyHandled, AlreadyShowcased, NotFound }

/// <summary>
/// How a review, edit or removal went. On <see cref="ReviewOutcome.Invalid"/>, <see cref="Errors"/> says
/// why, by form field.
/// </summary>
public sealed class ReviewResult
{
    public ReviewOutcome Outcome { get; init; }

    /// <summary>Problems by form field; the empty key is for the form as a whole.</summary>
    public Dictionary<string, string> Errors { get; } = new();

    /// <summary>For <see cref="ReviewOutcome.AlreadyHandled"/>: what happened to it, and who did it.</summary>
    public string? Message { get; init; }
}

/// <summary>
/// Reviewers' work: approving submissions into the showcase or declining them, and correcting or
/// removing storyboards that are in the showcase already.
/// </summary>
public class ReviewService(OsbDbContext db, IOsuWebHelper osu, ILogger<ReviewService> logger)
{
    public const int RecentCount = 20;

    /// <summary>
    /// How many submissions wait for review. The nav shows it to reviewers on every page, error pages
    /// included, so a database problem only hides the number.
    /// </summary>
    public async Task<int> CountWaitingAsync(CancellationToken ct = default)
    {
        try
        {
            return await db.Submissions.CountAsync(s => s.Status == SubmissionStatus.Pending, ct);
        }
        catch (Exception e) when (e is DbException or InvalidOperationException)
        {
            logger.LogWarning(e, "Couldn't count the submissions waiting for review.");
            return 0;
        }
    }

    /// <summary>Submissions waiting, oldest first, and the most recently reviewed or withdrawn ones.</summary>
    public async Task<(List<ShowcaseSubmission> Waiting, List<ShowcaseSubmission> Recent)> GetQueueAsync(CancellationToken ct = default)
    {
        var waiting = await WithDetails
            .Where(s => s.Status == SubmissionStatus.Pending)
            .OrderBy(s => s.SubmittedAt).ThenBy(s => s.Id)
            .ToListAsync(ct);
        var recent = await WithDetails
            .Where(s => s.Status != SubmissionStatus.Pending)
            .OrderByDescending(s => s.ReviewedAt).ThenByDescending(s => s.Id)
            .Take(RecentCount)
            .ToListAsync(ct);
        return (waiting, recent);
    }

    public Task<ShowcaseSubmission?> GetAsync(int id, CancellationToken ct = default) =>
        WithDetails.FirstOrDefaultAsync(s => s.Id == id, ct);

    /// <summary>Other submissions of the same beatmapset, newest first: earlier attempts and their reviews.</summary>
    public Task<List<ShowcaseSubmission>> GetEarlierAttemptsAsync(ShowcaseSubmission submission, CancellationToken ct = default) =>
        db.Submissions.AsNoTracking()
            .Where(s => s.BeatmapsetId == submission.BeatmapsetId && s.Id != submission.Id)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(ct);

    /// <summary>The mapper and storyboarders the site doesn't know yet; approving adds them.</summary>
    public async Task<List<string>> GetNewPeopleAsync(ShowcaseSubmission submission, CancellationToken ct = default)
    {
        var named = submission.Credits.Select(c => (c.OsuUserId, c.Username)).Prepend((submission.HostId, submission.HostUsername)).ToList();
        var ids = named.Select(n => n.Item1).ToList();
        var known = (await db.Users.Where(u => ids.Contains(u.Id)).Select(u => u.Id).ToListAsync(ct)).ToHashSet();
        return named.Where(n => !known.Contains(n.Item1)).Select(n => n.Item2).Distinct().ToList();
    }

    /// <summary>Who removed the storyboard from the showcase, when and why; null if nobody did.</summary>
    public Task<ShowcaseRemoval?> GetRemovalAsync(int beatmapsetId, CancellationToken ct = default) =>
        db.ShowcaseRemovals.AsNoTracking().FirstOrDefaultAsync(r => r.BeatmapsetId == beatmapsetId, ct);

    /// <summary>The review form as the submission fills it in, to be showcased today (UTC).</summary>
    public static ReviewForm FormFor(ShowcaseSubmission submission) => new()
    {
        Title = submission.Title,
        Artist = submission.Artist,
        Mapper = submission.HostUsername,
        Storyboarders = string.Join(", ", submission.Credits.Select(c => c.Username)),
        Medium = submission.Medium,
        Video = submission.VideoUrl,
        Tags = submission.SuggestedTags.Select(t => t.Slug).ToList(),
        SubmittedOn = submission.BeatmapSubmittedOn,
        ShowcasedOn = DateOnly.FromDateTime(DateTime.UtcNow),
    };

    /// <summary>The edit form, filled in with the storyboard as it is now.</summary>
    public static EntryForm FormFor(Beatmapset set) => new()
    {
        Title = set.Title,
        Artist = set.Artist,
        Mapper = set.Host.Username,
        Storyboarders = string.Join(", ", set.Credits.Select(u => u.Username)),
        Medium = set.Medium,
        Video = set.VideoUrl,
        Tags = set.Tags.Select(t => t.Slug).ToList(),
        SubmittedOn = set.SubmittedOn,
        ShowcasedOn = set.ShowcasedOn,
    };

    /// <summary>
    /// Adds the storyboard to the showcase as the form describes it, and marks the submission approved.
    /// People new to the site are added, but not as community members.
    /// </summary>
    public async Task<ReviewResult> ApproveAsync(int id, ReviewForm form, Member reviewer, CancellationToken ct = default)
    {
        var submission = await WithDetails.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (submission == null)
            return new ReviewResult { Outcome = ReviewOutcome.NotFound };
        if (submission.Status != SubmissionStatus.Pending)
            return Handled(submission);
        if (await db.Beatmapsets.AnyAsync(s => s.Id == submission.BeatmapsetId, ct))
            return new ReviewResult { Outcome = ReviewOutcome.AlreadyShowcased };

        var result = new ReviewResult { Outcome = ReviewOutcome.Invalid };
        var people = new PeopleResolver(db, osu);
        var known = submission.Credits.Select(c => new Person(c.OsuUserId, c.Username)).Prepend(new Person(submission.HostId, submission.HostUsername)).ToList();
        var entry = await CheckAsync(people, form, known, result.Errors, ct);
        if (entry == null)
            return result;

        var tags = await TagsAsync(form, ct);
        string medium = await SubmissionService.KnownMediumAsync(db, form.Medium, ct);
        var now = DateTime.UtcNow;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Claim the submission first, so a second tab (or reviewer) can't approve or decline it as well.
        if (!await ConcludeAsync(id, SubmissionStatus.Approved, reviewer, form.Message, now, ct))
            return Handled(await db.Submissions.AsNoTracking().FirstAsync(s => s.Id == id, ct));

        var users = await people.SaveAsync(entry.Storyboarders.Prepend(entry.Mapper), ct);
        db.Beatmapsets.Add(new Beatmapset
        {
            Id = submission.BeatmapsetId,
            Title = form.Title.Trim(),
            Artist = form.Artist.Trim(),
            Host = users[entry.Mapper.Id],
            Medium = medium,
            SubmittedOn = form.SubmittedOn!.Value,
            ShowcasedOn = form.ShowcasedOn!.Value,
            VideoUrl = entry.VideoUrl,
            ChangedOnSiteAt = now,
            Tags = tags,
            Storyboarders = entry.Storyboarders.Select((p, i) => new BeatmapsetStoryboarder { User = users[p.Id], Position = i }).ToList(),
        });
        // A storyboard removed earlier can come back through a new submission.
        if (await db.ShowcaseRemovals.FindAsync([submission.BeatmapsetId], ct) is { } removal)
            db.ShowcaseRemovals.Remove(removal);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (!await db.Beatmapsets.AsNoTracking().AnyAsync(s => s.Id == submission.BeatmapsetId, ct))
                throw;
            // It reached the showcase another way a moment ago. The transaction rolls back, approval included.
            return new ReviewResult { Outcome = ReviewOutcome.AlreadyShowcased };
        }
        await transaction.CommitAsync(ct);
        return new ReviewResult { Outcome = ReviewOutcome.Approved };
    }

    /// <summary>Declines a submission. The member sees the message, so it's required.</summary>
    public async Task<ReviewResult> DeclineAsync(int id, string? message, Member reviewer, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            var result = new ReviewResult { Outcome = ReviewOutcome.Invalid };
            result.Errors[nameof(ReviewForm.Message)] = "Tell the member why, so they know what to change.";
            return result;
        }
        if (await ConcludeAsync(id, SubmissionStatus.Declined, reviewer, message, DateTime.UtcNow, ct))
            return new ReviewResult { Outcome = ReviewOutcome.Declined };

        var submission = await db.Submissions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        return submission == null ? new ReviewResult { Outcome = ReviewOutcome.NotFound } : Handled(submission);
    }

    /// <summary>
    /// Changes a showcased storyboard to what the form describes. It's then marked as changed on the site,
    /// so showcase.json leaves it alone (see <see cref="ShowcaseSeeder"/>).
    /// </summary>
    public async Task<ReviewResult> EditAsync(int beatmapsetId, EntryForm form, CancellationToken ct = default)
    {
        var set = await db.Beatmapsets
            .Include(s => s.Host)
            .Include(s => s.Tags)
            .Include(s => s.Storyboarders).ThenInclude(c => c.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == beatmapsetId, ct);
        if (set == null)
            return new ReviewResult { Outcome = ReviewOutcome.NotFound };

        var result = new ReviewResult { Outcome = ReviewOutcome.Invalid };
        var people = new PeopleResolver(db, osu);
        var known = set.Credits.Prepend(set.Host).Select(u => new Person(u.Id, u.Username)).ToList();
        var entry = await CheckAsync(people, form, known, result.Errors, ct);
        if (entry == null)
            return result;

        var users = await people.SaveAsync(entry.Storyboarders.Prepend(entry.Mapper), ct);
        set.Title = form.Title.Trim();
        set.Artist = form.Artist.Trim();
        set.Host = users[entry.Mapper.Id];
        set.Medium = await SubmissionService.KnownMediumAsync(db, form.Medium, ct);
        set.SubmittedOn = form.SubmittedOn!.Value;
        set.ShowcasedOn = form.ShowcasedOn!.Value;
        set.VideoUrl = entry.VideoUrl;
        set.ChangedOnSiteAt = DateTime.UtcNow;

        // Tags and credits change in place: the rows of those that stay are kept, not deleted and added again.
        var tags = await TagsAsync(form, ct);
        var hadTags = set.Tags.Select(t => t.Id).ToHashSet();
        set.Tags.RemoveAll(t => tags.All(keep => keep.Id != t.Id));
        set.Tags.AddRange(tags.Where(t => !hadTags.Contains(t.Id)));

        var order = entry.Storyboarders.Select(p => p.Id).ToList();
        set.Storyboarders.RemoveAll(c => !order.Contains(c.OsuUserId));
        for (int position = 0; position < order.Count; position++)
        {
            if (set.Storyboarders.Find(c => c.OsuUserId == order[position]) is { } credit)
                credit.Position = position;
            else
                set.Storyboarders.Add(new BeatmapsetStoryboarder { User = users[order[position]], Position = position });
        }

        await db.SaveChangesAsync(ct);
        return new ReviewResult { Outcome = ReviewOutcome.Saved };
    }

    /// <summary>
    /// Takes a storyboard out of the showcase and remembers who did it and why, so showcase.json can't
    /// bring it back; a new submission can. Its people and tags stay on the site.
    /// </summary>
    public async Task<ReviewResult> RemoveAsync(int beatmapsetId, string? reason, Member reviewer, CancellationToken ct = default)
    {
        string why = reason?.Trim() ?? "";
        if (why.Length is 0 or > 1000)
        {
            var invalid = new ReviewResult { Outcome = ReviewOutcome.Invalid };
            invalid.Errors["Reason"] = why.Length == 0 ? "Give a reason for removing it." : "Keep the reason to 1,000 characters.";
            return invalid;
        }

        // Loaded with its credits and tag links, so they're deleted along with it.
        var set = await db.Beatmapsets
            .Include(s => s.Storyboarders)
            .Include(s => s.Tags)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == beatmapsetId, ct);
        if (set == null)
            return new ReviewResult { Outcome = ReviewOutcome.NotFound };

        db.Beatmapsets.Remove(set);
        var removal = await db.ShowcaseRemovals.FindAsync([beatmapsetId], ct);
        if (removal == null)
            db.ShowcaseRemovals.Add(removal = new ShowcaseRemoval { BeatmapsetId = beatmapsetId });
        removal.RemovedAt = DateTime.UtcNow;
        removal.RemovedById = reviewer.Id;
        removal.RemovedByUsername = reviewer.Username;
        removal.Reason = why;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (await db.Beatmapsets.AsNoTracking().AnyAsync(s => s.Id == beatmapsetId, ct))
                throw;
            // Removed a moment ago, from another tab.
            return new ReviewResult { Outcome = ReviewOutcome.NotFound };
        }
        return new ReviewResult { Outcome = ReviewOutcome.Removed };
    }

    private IQueryable<ShowcaseSubmission> WithDetails => db.Submissions.AsNoTracking()
        .Include(s => s.Storyboarders)
        .Include(s => s.SuggestedTags)
        .AsSplitQuery();

    /// <summary>A showcase entry's video and people, checked: one mapper, and the storyboarders in credit order.</summary>
    private sealed record CheckedEntry(string? VideoUrl, Person Mapper, IReadOnlyList<Person> Storyboarders);

    /// <summary>
    /// Checks what the form's own rules can't: the video link, and the mapper and storyboarders, who are
    /// looked up (see <see cref="PeopleResolver"/>). On a problem, adds it to <paramref name="errors"/> and
    /// returns null.
    /// </summary>
    private static async Task<CheckedEntry?> CheckAsync(PeopleResolver people, EntryForm form, IReadOnlyList<Person> known, Dictionary<string, string> errors, CancellationToken ct)
    {
        string? video = null;
        if (!string.IsNullOrWhiteSpace(form.Video) && (video = Format.YouTubeEmbedUrl(form.Video)) == null)
            errors[nameof(EntryForm.Video)] = "Use a YouTube link, like https://youtu.be/dQw4w9WgXcQ.";

        var (mappers, mapperError) = await people.ResolveAsync(form.Mapper, known, ct);
        if (mapperError != null)
            errors[nameof(EntryForm.Mapper)] = mapperError;
        else if (mappers.Count != 1)
            errors[nameof(EntryForm.Mapper)] = "Give one mapper.";
        var (storyboarders, storyboarderError) = await people.ResolveAsync(form.Storyboarders, known, ct);
        if (storyboarderError != null)
            errors[nameof(EntryForm.Storyboarders)] = storyboarderError;
        else if (storyboarders.Count == 0)
            errors[nameof(EntryForm.Storyboarders)] = "List at least one storyboarder.";
        else if (storyboarders.Count > SubmissionService.MaxStoryboarders)
            errors[nameof(EntryForm.Storyboarders)] = $"List up to {SubmissionService.MaxStoryboarders} storyboarders.";

        return errors.Count > 0 ? null : new CheckedEntry(video, mappers[0], storyboarders);
    }

    /// <summary>The tags the form ticks.</summary>
    private async Task<List<StoryboardTag>> TagsAsync(EntryForm form, CancellationToken ct) =>
        form.Tags.Count > 0 ? await db.Tags.Where(t => form.Tags.Contains(t.Slug)).ToListAsync(ct) : [];

    /// <summary>Moves a waiting submission to its outcome; false when it was no longer waiting.</summary>
    private async Task<bool> ConcludeAsync(int id, SubmissionStatus outcome, Member reviewer, string? message, DateTime now, CancellationToken ct)
    {
        string? note = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        return await db.Submissions
            .Where(s => s.Id == id && s.Status == SubmissionStatus.Pending)
            .ExecuteUpdateAsync(set => set
                .SetProperty(s => s.Status, outcome)
                .SetProperty(s => s.ReviewerId, reviewer.Id)
                .SetProperty(s => s.ReviewerUsername, reviewer.Username)
                .SetProperty(s => s.ReviewedAt, now)
                .SetProperty(s => s.ReviewNote, note)
                .SetProperty(s => s.OutcomeSeen, false), ct) > 0;
    }

    private static ReviewResult Handled(ShowcaseSubmission submission) => new()
    {
        Outcome = ReviewOutcome.AlreadyHandled,
        Message = submission.Status switch
        {
            SubmissionStatus.Approved => $"{submission.ReviewerUsername} already approved {submission.Artist} - {submission.Title}.",
            SubmissionStatus.Declined => $"{submission.ReviewerUsername} already declined {submission.Artist} - {submission.Title}.",
            _ => $"{submission.SubmitterUsername} withdrew {submission.Artist} - {submission.Title}.",
        },
    };
}
