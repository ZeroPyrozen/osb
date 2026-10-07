#nullable enable

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.ViewModels;

namespace osb.Services;

public enum ReviewOutcome { Approved, Declined, Invalid, AlreadyHandled, AlreadyShowcased, NotFound }

/// <summary>How a review went. On <see cref="ReviewOutcome.Invalid"/>, <see cref="Errors"/> says why, by form field.</summary>
public sealed class ReviewResult
{
    public ReviewOutcome Outcome { get; init; }

    /// <summary>Problems by form field; the empty key is for the form as a whole.</summary>
    public Dictionary<string, string> Errors { get; } = new();

    /// <summary>For <see cref="ReviewOutcome.AlreadyHandled"/>: what happened to it, and who did it.</summary>
    public string? Message { get; init; }
}

/// <summary>The review queue: reviewers approving submissions into the showcase, or declining them.</summary>
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
        string? video = null;
        if (!string.IsNullOrWhiteSpace(form.Video) && (video = Format.YouTubeEmbedUrl(form.Video)) == null)
            result.Errors[nameof(EntryForm.Video)] = "Use a YouTube link, like https://youtu.be/dQw4w9WgXcQ.";

        var people = new PeopleResolver(db, osu);
        var known = submission.Credits.Select(c => new Person(c.OsuUserId, c.Username)).Prepend(new Person(submission.HostId, submission.HostUsername)).ToList();
        var (mappers, mapperError) = await people.ResolveAsync(form.Mapper, known, ct);
        if (mapperError != null)
            result.Errors[nameof(EntryForm.Mapper)] = mapperError;
        else if (mappers.Count != 1)
            result.Errors[nameof(EntryForm.Mapper)] = "Give one mapper.";
        var (storyboarders, storyboarderError) = await people.ResolveAsync(form.Storyboarders, known, ct);
        if (storyboarderError != null)
            result.Errors[nameof(EntryForm.Storyboarders)] = storyboarderError;
        else if (storyboarders.Count == 0)
            result.Errors[nameof(EntryForm.Storyboarders)] = "List at least one storyboarder.";
        else if (storyboarders.Count > SubmissionService.MaxStoryboarders)
            result.Errors[nameof(EntryForm.Storyboarders)] = $"List up to {SubmissionService.MaxStoryboarders} storyboarders.";
        if (result.Errors.Count > 0)
            return result;

        var tags = form.Tags.Count > 0 ? await db.Tags.Where(t => form.Tags.Contains(t.Slug)).ToListAsync(ct) : [];
        string medium = await SubmissionService.KnownMediumAsync(db, form.Medium, ct);
        var now = DateTime.UtcNow;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Claim the submission first, so a second tab (or reviewer) can't approve or decline it as well.
        if (!await ConcludeAsync(id, SubmissionStatus.Approved, reviewer, form.Message, now, ct))
            return Handled(await db.Submissions.AsNoTracking().FirstAsync(s => s.Id == id, ct));

        var users = await people.SaveAsync(mappers.Concat(storyboarders), ct);
        db.Beatmapsets.Add(new Beatmapset
        {
            Id = submission.BeatmapsetId,
            Title = form.Title.Trim(),
            Artist = form.Artist.Trim(),
            Host = users[mappers[0].Id],
            Medium = medium,
            SubmittedOn = form.SubmittedOn!.Value,
            ShowcasedOn = form.ShowcasedOn!.Value,
            VideoUrl = video,
            ChangedOnSiteAt = now,
            Tags = tags,
            Storyboarders = storyboarders.Select((p, i) => new BeatmapsetStoryboarder { User = users[p.Id], Position = i }).ToList(),
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

    private IQueryable<ShowcaseSubmission> WithDetails => db.Submissions.AsNoTracking()
        .Include(s => s.Storyboarders)
        .Include(s => s.SuggestedTags)
        .AsSplitQuery();

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
