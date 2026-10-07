#nullable enable

using System.Data.Common;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.ViewModels;

namespace osb.Services;

/// <summary>Someone logged in with osu!, as their login cookie names them.</summary>
public sealed record Member(int Id, string Username)
{
    public static Member From(ClaimsPrincipal user) =>
        new(int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!), user.Identity?.Name ?? "");
}

/// <summary>How a submission went: saved, or stopped by the problems in <see cref="Errors"/>.</summary>
public sealed class SubmitResult
{
    /// <summary>The new submission's ID, once it's saved.</summary>
    public int? SubmissionId { get; init; }

    /// <summary>Problems by form field; the empty key is for the form as a whole.</summary>
    public Dictionary<string, string> Errors { get; } = new();

    /// <summary>Set when the storyboard is already in the showcase, so the page can link to it.</summary>
    public int? ShowcasedId { get; set; }
}

public enum WithdrawOutcome { Withdrawn, AlreadyReviewed, NotFound }

/// <summary>Members submitting storyboards for the showcase, following them and withdrawing them.</summary>
public class SubmissionService(OsbDbContext db, IOsuWebHelper osu, ILogger<SubmissionService> logger)
{
    public const int MaxWaitingPerMember = 5;
    public const int MaxStoryboarders = 10;

    private static readonly SubmissionStatus[] Reviewed = [SubmissionStatus.Approved, SubmissionStatus.Declined];

    /// <summary>
    /// Checks a submission and saves it as waiting for review. The title, artist, mapper and submission
    /// date come from osu!, and each storyboarder is looked up there.
    /// </summary>
    public async Task<SubmitResult> SubmitAsync(SubmitForm form, Member member, CancellationToken ct = default)
    {
        var result = new SubmitResult();
        var link = OsuLinks.ParseBeatmap(form.Beatmapset);
        if (link == null)
            result.Errors[nameof(SubmitForm.Beatmapset)] = "Paste a link to the beatmapset, like https://osu.ppy.sh/beatmapsets/1011020, or its ID.";
        var names = OsuLinks.ParseUsers(form.Storyboarders);
        if (names.Count == 0)
            result.Errors[nameof(SubmitForm.Storyboarders)] = "List at least one storyboarder.";
        else if (names.Count > MaxStoryboarders)
            result.Errors[nameof(SubmitForm.Storyboarders)] = $"List up to {MaxStoryboarders} storyboarders.";
        string? video = null;
        if (!string.IsNullOrWhiteSpace(form.Video) && (video = Format.YouTubeEmbedUrl(form.Video)) == null)
            result.Errors[nameof(SubmitForm.Video)] = "Use a YouTube link, like https://youtu.be/dQw4w9WgXcQ.";
        if (result.Errors.Count > 0)
            return result;

        if (await db.Submissions.CountAsync(s => s.SubmitterId == member.Id && s.Status == SubmissionStatus.Pending, ct) >= MaxWaitingPerMember)
            return Fail(result, "", $"You have {MaxWaitingPerMember} submissions waiting for review. You can add more once they're reviewed.");

        int beatmapsetId = link!.Value.Id;
        if (link.Value.IsDifficulty)
        {
            var beatmap = await osu.GetBeatmapData(beatmapsetId);
            if (beatmap.Unavailable)
                return Unreachable(result);
            if (beatmap.Value == null)
                return Fail(result, nameof(SubmitForm.Beatmapset), $"osu! has no beatmap {beatmapsetId}.");
            beatmapsetId = beatmap.Value.BeatmapsetID;
        }

        if (await db.Beatmapsets.AnyAsync(s => s.Id == beatmapsetId, ct))
        {
            result.ShowcasedId = beatmapsetId;
            return Fail(result, nameof(SubmitForm.Beatmapset), "This storyboard is already in the showcase.");
        }
        if (await IsWaitingAsync(beatmapsetId, ct))
            return Fail(result, nameof(SubmitForm.Beatmapset), "This storyboard is already waiting for review.");

        var set = await osu.GetBeatmapsetData(beatmapsetId);
        if (set.Unavailable)
            return Unreachable(result);
        if (set.Value == null)
            return Fail(result, nameof(SubmitForm.Beatmapset), $"osu! has no beatmapset {beatmapsetId}.");

        var credits = new List<SubmissionCredit>();
        var unknown = new List<string>();
        foreach (string name in names)
        {
            var person = await osu.FindUser(name);
            if (person.Unavailable)
                return Unreachable(result);
            if (person.Value == null)
                unknown.Add(name);
            else if (credits.All(c => c.OsuUserId != person.Value.ID))
                credits.Add(new SubmissionCredit { OsuUserId = person.Value.ID, Username = person.Value.Username, Position = credits.Count });
        }
        if (unknown.Count > 0)
            return Fail(result, nameof(SubmitForm.Storyboarders), $"osu! has no player called {Format.List(unknown)}.");

        var submission = new ShowcaseSubmission
        {
            BeatmapsetId = beatmapsetId,
            Title = set.Value.Title,
            Artist = set.Value.Artist,
            HostId = set.Value.UserID,
            HostUsername = set.Value.Creator,
            BeatmapSubmittedOn = DateOnly.FromDateTime((set.Value.SubmittedDate ?? DateTimeOffset.UtcNow).UtcDateTime),
            OsuListsStoryboard = set.Value.Storyboard,
            Medium = await KnownMediumAsync(db, form.Medium, ct),
            VideoUrl = video,
            Note = string.IsNullOrWhiteSpace(form.Note) ? null : form.Note.Trim(),
            Storyboarders = credits,
            SuggestedTags = form.Tags.Count > 0 ? await db.Tags.Where(t => form.Tags.Contains(t.Slug)).ToListAsync(ct) : [],
            SubmitterId = member.Id,
            SubmitterUsername = member.Username,
            SubmittedAt = DateTime.UtcNow,
            Status = SubmissionStatus.Pending,
        };
        db.Submissions.Add(submission);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Someone else submitted the same beatmapset a moment ago; the database allows one waiting.
            if (await IsWaitingAsync(beatmapsetId, ct))
                return Fail(result, nameof(SubmitForm.Beatmapset), "This storyboard is already waiting for review.");
            throw;
        }
        return new SubmitResult { SubmissionId = submission.Id };
    }

    /// <summary>The member's submissions, newest first, with their credits.</summary>
    public async Task<List<ShowcaseSubmission>> GetMineAsync(int memberId, CancellationToken ct = default) =>
        await db.Submissions.AsNoTracking()
            .Include(s => s.Storyboarders)
            .Where(s => s.SubmitterId == memberId)
            .OrderByDescending(s => s.SubmittedAt).ThenByDescending(s => s.Id)
            .ToListAsync(ct);

    /// <summary>Which of these beatmapsets are in the showcase now; an approved one may have been removed since.</summary>
    public async Task<HashSet<int>> ShowcasedAsync(IEnumerable<int> beatmapsetIds, CancellationToken ct = default)
    {
        var ids = beatmapsetIds.Distinct().ToList();
        return (await db.Beatmapsets.Where(s => ids.Contains(s.Id)).Select(s => s.Id).ToListAsync(ct)).ToHashSet();
    }

    /// <summary>Notes that the member has seen how their submissions were reviewed, which takes away the dot.</summary>
    public Task MarkOutcomesSeenAsync(int memberId, CancellationToken ct = default) =>
        Unseen(memberId).ExecuteUpdateAsync(set => set.SetProperty(s => s.OutcomeSeen, true), ct);

    /// <summary>
    /// Whether a submission of the member's was reviewed since they last looked. The nav asks on every page
    /// for logged-in members, error pages included, so a database problem only hides the dot.
    /// </summary>
    public async Task<bool> HasUnseenOutcomesAsync(int memberId, CancellationToken ct = default)
    {
        try
        {
            return await Unseen(memberId).AnyAsync(ct);
        }
        catch (Exception e) when (e is DbException or InvalidOperationException)
        {
            logger.LogWarning(e, "Couldn't check whether member {Member} has reviewed submissions to see.", memberId);
            return false;
        }
    }

    /// <summary>Withdraws a member's own submission while it's still waiting for review.</summary>
    public async Task<WithdrawOutcome> WithdrawAsync(int submissionId, int memberId, CancellationToken ct = default)
    {
        // One conditional update, so a review at the same moment can't be overwritten.
        int changed = await db.Submissions
            .Where(s => s.Id == submissionId && s.SubmitterId == memberId && s.Status == SubmissionStatus.Pending)
            .ExecuteUpdateAsync(set => set
                .SetProperty(s => s.Status, SubmissionStatus.Withdrawn)
                .SetProperty(s => s.ReviewedAt, DateTime.UtcNow)
                .SetProperty(s => s.OutcomeSeen, true), ct);
        if (changed > 0)
            return WithdrawOutcome.Withdrawn;
        return await db.Submissions.AnyAsync(s => s.Id == submissionId && s.SubmitterId == memberId, ct)
            ? WithdrawOutcome.AlreadyReviewed
            : WithdrawOutcome.NotFound;
    }

    private IQueryable<ShowcaseSubmission> Unseen(int memberId) =>
        db.Submissions.Where(s => s.SubmitterId == memberId && !s.OutcomeSeen && Reviewed.Contains(s.Status));

    private Task<bool> IsWaitingAsync(int beatmapsetId, CancellationToken ct) =>
        db.Submissions.AnyAsync(s => s.BeatmapsetId == beatmapsetId && s.Status == SubmissionStatus.Pending, ct);

    /// <summary>The tool as the showcase already spells it ("storybrew" becomes "Storybrew"), or as typed.</summary>
    internal static async Task<string> KnownMediumAsync(OsbDbContext db, string medium, CancellationToken ct)
    {
        string typed = medium.Trim();
        var known = await db.Beatmapsets.Select(s => s.Medium).Distinct().ToListAsync(ct);
        return known.FirstOrDefault(m => string.Equals(m, typed, StringComparison.OrdinalIgnoreCase)) ?? typed;
    }

    private static SubmitResult Fail(SubmitResult result, string field, string message)
    {
        result.Errors[field] = message;
        return result;
    }

    private static SubmitResult Unreachable(SubmitResult result) =>
        Fail(result, "", "Couldn't reach osu! Try again in a few minutes.");
}
