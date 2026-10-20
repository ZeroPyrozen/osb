using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Tests.Infrastructure;

namespace osb.Tests.Data;

/// <summary>How submissions are stored, on the schema the migrations build.</summary>
public class SubmissionModelTests
{
    private static ShowcaseSubmission Submission(int beatmapsetId, SubmissionStatus status) => new()
    {
        BeatmapsetId = beatmapsetId,
        Title = "Song",
        Artist = "Artist",
        HostId = 4,
        HostUsername = "Mapper",
        BeatmapSubmittedOn = new DateOnly(2024, 1, 1),
        Medium = "Storybrew",
        SubmitterId = 2,
        SubmitterUsername = "bob",
        SubmittedAt = new DateTime(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc),
        Status = status,
    };

    [Fact]
    public async Task OnlyOneSubmissionPerBeatmapset_CanWaitForReview()
    {
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
        {
            db.Submissions.AddRange(Submission(1011020, SubmissionStatus.Declined), Submission(1011020, SubmissionStatus.Pending));
            await db.SaveChangesAsync();
        }

        await using (var db = database.CreateContext())
        {
            db.Submissions.Add(Submission(1011020, SubmissionStatus.Pending));
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }

        // Finished submissions, and other beatmapsets, are no problem.
        await using (var db = database.CreateContext())
        {
            db.Submissions.AddRange(Submission(1011020, SubmissionStatus.Withdrawn), Submission(1011020, SubmissionStatus.Approved), Submission(292301, SubmissionStatus.Pending));
            await db.SaveChangesAsync();
        }
        await using var check = database.CreateContext();
        Assert.Equal(5, await check.Submissions.CountAsync());
    }

    [Fact]
    public async Task ASubmission_KeepsItsCreditsInOrder_AndItsSuggestedTags()
    {
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
        {
            var particles = new StoryboardTag { Slug = "particles", Name = "Particles", Rating = 15 };
            var lyrics = new StoryboardTag { Slug = "lyrics", Name = "Lyrics", Rating = 10 };
            var submission = Submission(1011020, SubmissionStatus.Pending);
            submission.Storyboarders =
            [
                new SubmissionCredit { OsuUserId = 9, Username = "second", Position = 1 },
                new SubmissionCredit { OsuUserId = 7, Username = "first", Position = 0 },
            ];
            submission.SuggestedTags = [particles, lyrics];
            db.Submissions.Add(submission);
            await db.SaveChangesAsync();
        }

        await using var check = database.CreateContext();
        var saved = await check.Submissions.Include(s => s.Storyboarders).Include(s => s.SuggestedTags).SingleAsync();

        Assert.Equal(["first", "second"], saved.Credits.Select(c => c.Username));
        Assert.Equal(["lyrics", "particles"], saved.SuggestedTags.Select(t => t.Slug).Order());
        Assert.False(saved.OutcomeSeen);
    }

    [Fact]
    public async Task ARemoval_IsRememberedByBeatmapset()
    {
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
        {
            db.ShowcaseRemovals.Add(new ShowcaseRemoval { BeatmapsetId = 1011020, RemovedAt = DateTime.UtcNow, RemovedById = 2, RemovedByUsername = "reviewer", Reason = "Deleted from osu!." });
            await db.SaveChangesAsync();
        }

        await using var check = database.CreateContext();
        Assert.Equal("Deleted from osu!.", (await check.ShowcaseRemovals.FindAsync(1011020))!.Reason);
    }
}
