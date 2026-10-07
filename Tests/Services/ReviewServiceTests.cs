using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using osb.Data;
using osb.Helpers;
using osb.Services;
using osb.Tests.Infrastructure;
using osb.ViewModels;

namespace osb.Tests.Services;

/// <summary>Approving and declining submissions, against a small showcase and the fake osu!.</summary>
public sealed class ReviewServiceTests : IDisposable
{
    private static readonly Member Reviewer = new(9001, "Reviewer");
    private static readonly DateOnly Today = new(2026, 10, 7);

    private readonly TestDatabase database = new();
    private readonly FakeOsu osu = new();

    public ReviewServiceTests()
    {
        using var db = database.CreateContext();
        var mentor = new CommunityRole { Name = "Mentor", Colour = "#2ecc71", Rank = 6 };
        db.Tags.AddRange(
            new StoryboardTag { Slug = "particles", Name = "Particles", Rating = 15 },
            new StoryboardTag { Slug = "lyrics", Name = "Lyrics", Rating = 10 });
        db.Users.Add(new OsuUser { Id = 4, Username = "Dave", IsCommunityMember = true, Roles = [mentor] });
        db.Beatmapsets.Add(new Beatmapset
        {
            Id = 100, Title = "Already shown", Artist = "Artist", Host = new OsuUser { Id = 5, Username = "Host" }, Medium = "Scripting",
            SubmittedOn = new DateOnly(2020, 1, 1), ShowcasedOn = new DateOnly(2020, 2, 1),
        });
        db.SaveChanges();

        osu.Profiles[8001] = new WebUserModel { ID = 8001, Username = "NewPerson" };
    }

    public void Dispose() => database.Dispose();

    private ReviewService Service() => new(database.CreateContext(), osu, NullLogger<ReviewService>.Instance);

    /// <summary>A waiting submission: mapped by someone new, storyboarded by someone new and by Dave.</summary>
    private async Task<int> SubmissionAsync(int beatmapsetId = 1011020, int submitter = 2, DateTime? at = null, SubmissionStatus status = SubmissionStatus.Pending)
    {
        await using var db = database.CreateContext();
        var submission = new ShowcaseSubmission
        {
            BeatmapsetId = beatmapsetId, Title = "DYE/Re:flection+", Artist = "AVTechNO!xTreow", HostId = 6607303, HostUsername = "The Mapper",
            BeatmapSubmittedOn = new DateOnly(2019, 7, 30), OsuListsStoryboard = true, Medium = "Scripting",
            VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", SubmitterId = submitter, SubmitterUsername = submitter == 2 ? "bob" : "Reviewer",
            SubmittedAt = at ?? new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc), Status = status,
            ReviewedAt = status == SubmissionStatus.Pending ? null : at, ReviewerUsername = status == SubmissionStatus.Pending ? null : "Reviewer",
            Storyboarders =
            [
                new SubmissionCredit { OsuUserId = 7405768, Username = "Storyboarder", Position = 0 },
                new SubmissionCredit { OsuUserId = 4, Username = "Dave", Position = 1 },
            ],
            SuggestedTags = await db.Tags.Where(t => t.Slug == "particles").ToListAsync(),
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();
        return submission.Id;
    }

    private async Task<ReviewForm> FormAsync(int id)
    {
        var form = ReviewService.FormFor((await Service().GetAsync(id))!);
        form.ShowcasedOn = Today;
        return form;
    }

    private async Task<Beatmapset> ShowcasedAsync(int beatmapsetId)
    {
        await using var db = database.CreateContext();
        return await db.Beatmapsets.Include(s => s.Host).Include(s => s.Tags).Include(s => s.Storyboarders).ThenInclude(c => c.User)
            .AsSplitQuery().SingleAsync(s => s.Id == beatmapsetId);
    }

    private async Task<ShowcaseSubmission> SubmissionByIdAsync(int id)
    {
        await using var db = database.CreateContext();
        return await db.Submissions.SingleAsync(s => s.Id == id);
    }

    [Fact]
    public async Task TheForm_StartsFromTheSubmission_ShowcasedToday()
    {
        var form = ReviewService.FormFor((await Service().GetAsync(await SubmissionAsync()))!);

        Assert.Equal(("DYE/Re:flection+", "The Mapper", "Storyboarder, Dave", "Scripting"), (form.Title, form.Mapper, form.Storyboarders, form.Medium));
        Assert.Equal(["particles"], form.Tags);
        Assert.Equal(new DateOnly(2019, 7, 30), form.SubmittedOn);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), form.ShowcasedOn);
    }

    [Fact]
    public async Task Approving_PutsTheStoryboardInTheShowcase_AsTheFormDescribesIt()
    {
        int id = await SubmissionAsync();
        var form = await FormAsync(id);
        form.Title = "DYE/Re:flection+ (edited)";
        form.Tags = ["particles", "lyrics"];
        form.Message = "  Lovely work.  ";

        var result = await Service().ApproveAsync(id, form, Reviewer);

        Assert.Equal(ReviewOutcome.Approved, result.Outcome);
        var set = await ShowcasedAsync(1011020);
        Assert.Equal(("DYE/Re:flection+ (edited)", "AVTechNO!xTreow", "Scripting"), (set.Title, set.Artist, set.Medium));
        Assert.Equal((6607303, "The Mapper"), (set.Host.Id, set.Host.Username));
        Assert.Equal(["Storyboarder", "Dave"], set.Credits.Select(u => u.Username));
        Assert.Equal(25, set.OsbLevel);
        Assert.Equal((new DateOnly(2019, 7, 30), Today), (set.SubmittedOn, set.ShowcasedOn));
        Assert.Equal("https://www.youtube.com/embed/dQw4w9WgXcQ", set.VideoUrl);
        Assert.NotNull(set.ChangedOnSiteAt);

        var submission = await SubmissionByIdAsync(id);
        Assert.Equal((SubmissionStatus.Approved, 9001, "Reviewer", "Lovely work.", false), (submission.Status, submission.ReviewerId, submission.ReviewerUsername, submission.ReviewNote, submission.OutcomeSeen));
    }

    [Fact]
    public async Task NewPeople_JoinTheSite_ButNotTheCommunity_AndMembersKeepTheirRoles()
    {
        int id = await SubmissionAsync();

        await Service().ApproveAsync(id, await FormAsync(id), Reviewer);

        await using var db = database.CreateContext();
        var people = await db.Users.Include(u => u.Roles).Where(u => u.Id == 6607303 || u.Id == 7405768 || u.Id == 4).ToDictionaryAsync(u => u.Id);
        Assert.False(people[6607303].IsCommunityMember);
        Assert.False(people[7405768].IsCommunityMember);
        Assert.True(people[4].IsCommunityMember);
        Assert.Equal(["Mentor"], people[4].Roles.Select(r => r.Name));
    }

    [Fact]
    public async Task Approving_WorksWhileOsuIsDown_AsLongAsNoNameChanged()
    {
        int id = await SubmissionAsync();
        osu.Unreachable = true;

        Assert.Equal(ReviewOutcome.Approved, (await Service().ApproveAsync(id, await FormAsync(id), Reviewer)).Outcome);
    }

    [Fact]
    public async Task ANewName_IsLookedUpOnOsu_AndAKnownName_IsTakenFromTheSite()
    {
        int id = await SubmissionAsync();
        var form = await FormAsync(id);
        form.Mapper = "dave";
        form.Storyboarders = "NewPerson, Storyboarder";

        await Service().ApproveAsync(id, form, Reviewer);

        var set = await ShowcasedAsync(1011020);
        Assert.Equal(4, set.Host.Id);
        Assert.Equal([8001, 7405768], set.Credits.Select(u => u.Id));
    }

    [Fact]
    public async Task SomeoneRenamedOnOsu_GetsTheirNewName()
    {
        osu.Profiles[4] = new WebUserModel { ID = 4, Username = "Dave the Second" };
        int id = await SubmissionAsync();
        var form = await FormAsync(id);
        form.Storyboarders = "Storyboarder, Dave the Second";

        await Service().ApproveAsync(id, form, Reviewer);

        Assert.Equal(["Storyboarder", "Dave the Second"], (await ShowcasedAsync(1011020)).Credits.Select(u => u.Username));
    }

    [Theory]
    [InlineData("Ghost", "Storyboarder", "Mapper", "osu! has no player called Ghost.")]
    [InlineData("The Mapper, Dave", "Storyboarder", "Mapper", "Give one mapper.")]
    [InlineData("The Mapper", "Storyboarder, Ghost, Nobody", "Storyboarders", "osu! has no player called Ghost and Nobody.")]
    [InlineData("The Mapper", " , ", "Storyboarders", "List at least one storyboarder.")]
    public async Task NamesThatDontWork_AreExplainedOnTheirField_AndNothingChanges(string mapper, string storyboarders, string field, string message)
    {
        int id = await SubmissionAsync();
        var form = await FormAsync(id);
        form.Mapper = mapper;
        form.Storyboarders = storyboarders;

        var result = await Service().ApproveAsync(id, form, Reviewer);

        Assert.Equal(ReviewOutcome.Invalid, result.Outcome);
        Assert.Equal(message, Assert.Contains(field, result.Errors));
        Assert.Equal(SubmissionStatus.Pending, (await SubmissionByIdAsync(id)).Status);
        await using var db = database.CreateContext();
        Assert.False(await db.Beatmapsets.AnyAsync(s => s.Id == 1011020));
    }

    [Fact]
    public async Task ANewName_WhileOsuIsDown_SaysSo()
    {
        int id = await SubmissionAsync();
        var form = await FormAsync(id);
        form.Storyboarders = "Storyboarder, NewPerson";
        osu.Unreachable = true;

        var result = await Service().ApproveAsync(id, form, Reviewer);

        Assert.Equal("Couldn't reach osu! to look up NewPerson. Try again in a few minutes.", Assert.Contains("Storyboarders", result.Errors));
    }

    [Fact]
    public async Task AVideoLink_MustBeYouTube()
    {
        int id = await SubmissionAsync();
        var form = await FormAsync(id);
        form.Video = "https://vimeo.com/1";

        Assert.Contains("Video", (await Service().ApproveAsync(id, form, Reviewer)).Errors);
    }

    [Fact]
    public async Task ASubmission_IsOnlyReviewedOnce()
    {
        int id = await SubmissionAsync();
        await Service().ApproveAsync(id, await FormAsync(id), Reviewer);

        var again = await Service().ApproveAsync(id, await FormAsync(id), Reviewer);
        var decline = await Service().DeclineAsync(id, "Changed my mind.", Reviewer);

        Assert.Equal(ReviewOutcome.AlreadyHandled, again.Outcome);
        Assert.Equal("Reviewer already approved AVTechNO!xTreow - DYE/Re:flection+.", again.Message);
        Assert.Equal(ReviewOutcome.AlreadyHandled, decline.Outcome);
        Assert.Equal(SubmissionStatus.Approved, (await SubmissionByIdAsync(id)).Status);
    }

    [Fact]
    public async Task AStoryboardAlreadyInTheShowcase_CantBeApproved()
    {
        int id = await SubmissionAsync(beatmapsetId: 100);

        Assert.Equal(ReviewOutcome.AlreadyShowcased, (await Service().ApproveAsync(id, await FormAsync(id), Reviewer)).Outcome);
        Assert.Equal(SubmissionStatus.Pending, (await SubmissionByIdAsync(id)).Status);
    }

    [Fact]
    public async Task ARemovedStoryboard_ComesBack_WhenANewSubmissionIsApproved()
    {
        await using (var db = database.CreateContext())
        {
            db.ShowcaseRemovals.Add(new ShowcaseRemoval { BeatmapsetId = 1011020, RemovedAt = DateTime.UtcNow, RemovedByUsername = "Reviewer", Reason = "Video gone." });
            await db.SaveChangesAsync();
        }
        int id = await SubmissionAsync();

        await Service().ApproveAsync(id, await FormAsync(id), Reviewer);

        await using var check = database.CreateContext();
        Assert.False(await check.ShowcaseRemovals.AnyAsync());
        Assert.True(await check.Beatmapsets.AnyAsync(s => s.Id == 1011020));
    }

    [Fact]
    public async Task Reviewers_CanApproveTheirOwnSubmissions()
    {
        int id = await SubmissionAsync(submitter: Reviewer.Id);

        Assert.Equal(ReviewOutcome.Approved, (await Service().ApproveAsync(id, await FormAsync(id), Reviewer)).Outcome);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Declining_NeedsAMessage(string? message)
    {
        int id = await SubmissionAsync();

        var result = await Service().DeclineAsync(id, message, Reviewer);

        Assert.Equal("Tell the member why, so they know what to change.", Assert.Contains("Message", result.Errors));
        Assert.Equal(SubmissionStatus.Pending, (await SubmissionByIdAsync(id)).Status);
    }

    [Fact]
    public async Task Declining_LeavesTheMemberAMessage()
    {
        int id = await SubmissionAsync();

        var result = await Service().DeclineAsync(id, " Please add a video. ", Reviewer);

        Assert.Equal(ReviewOutcome.Declined, result.Outcome);
        var submission = await SubmissionByIdAsync(id);
        Assert.Equal((SubmissionStatus.Declined, "Please add a video.", "Reviewer", false), (submission.Status, submission.ReviewNote, submission.ReviewerUsername, submission.OutcomeSeen));
        Assert.Equal(ReviewOutcome.NotFound, (await Service().DeclineAsync(id + 100, "Nope.", Reviewer)).Outcome);
    }

    [Fact]
    public async Task TheQueue_IsOldestFirst_WithTheLatestReviewsBelow()
    {
        DateTime At(int day) => new(2026, 10, day, 12, 0, 0, DateTimeKind.Utc);
        int newer = await SubmissionAsync(beatmapsetId: 2, at: At(5));
        int older = await SubmissionAsync(beatmapsetId: 3, at: At(2));
        int reviewedEarlier = await SubmissionAsync(beatmapsetId: 4, at: At(1), status: SubmissionStatus.Declined);
        int reviewedLater = await SubmissionAsync(beatmapsetId: 5, at: At(3), status: SubmissionStatus.Withdrawn);

        var (waiting, recent) = await Service().GetQueueAsync();

        Assert.Equal([older, newer], waiting.Select(s => s.Id));
        Assert.Equal([reviewedLater, reviewedEarlier], recent.Select(s => s.Id));
        Assert.Equal(2, await Service().CountWaitingAsync());
    }

    [Fact]
    public async Task ThePage_PointsOutNewPeople_AndEarlierAttempts()
    {
        int earlier = await SubmissionAsync(status: SubmissionStatus.Declined, at: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        int id = await SubmissionAsync();
        var service = Service();
        var submission = (await service.GetAsync(id))!;

        Assert.Equal(["The Mapper", "Storyboarder"], await service.GetNewPeopleAsync(submission));
        Assert.Equal([earlier], (await service.GetEarlierAttemptsAsync(submission)).Select(s => s.Id));
    }
}
