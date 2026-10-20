using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using osb.Data;
using osb.Helpers;
using osb.Services;
using osb.Tests.Infrastructure;
using osb.ViewModels;

namespace osb.Tests.Services;

/// <summary>Submitting, following and withdrawing, against a small showcase and the fake osu!.</summary>
public sealed class SubmissionServiceTests : IDisposable
{
    private static readonly Member Bob = new(2, "bob");
    private static readonly Member Carol = new(3, "Carol");

    private readonly TestDatabase database = new();
    private readonly FakeOsu osu = new();

    public SubmissionServiceTests()
    {
        using var db = database.CreateContext();
        db.Tags.AddRange(
            new StoryboardTag { Slug = "particles", Name = "Particles", Rating = 15 },
            new StoryboardTag { Slug = "lyrics", Name = "Lyrics", Rating = 10 });
        db.Beatmapsets.Add(new Beatmapset
        {
            Id = 100, Title = "Already shown", Artist = "Artist", Host = new OsuUser { Id = 4, Username = "Dave" }, Medium = "Storybrew",
            SubmittedOn = new DateOnly(2020, 1, 1), ShowcasedOn = new DateOnly(2020, 2, 1),
        });
        db.SaveChanges();

        // Submitted to osu! late on 29 July in UTC-7, which is already 30 July in UTC.
        osu.Beatmapsets[1011020] = new WebBeatmapsetModel
        {
            ID = 1011020, Title = "DYE/Re:flection+", Artist = "AVTechNO!xTreow", Creator = "The Mapper", UserID = 6607303,
            Storyboard = true, SubmittedDate = new DateTimeOffset(2019, 7, 29, 23, 30, 0, TimeSpan.FromHours(-7)),
        };
        osu.Beatmapsets[292301] = new WebBeatmapsetModel { ID = 292301, Title = "Blue Zenith", Artist = "xi", Creator = "A Mapper", UserID = 1000, Storyboard = false };
        osu.Beatmapsets[409681] = new WebBeatmapsetModel { ID = 409681, Title = "Happy End of the World", Artist = "xi", Creator = "Another Mapper", UserID = 1001, Storyboard = true };
        osu.Beatmaps[2115170] = new WebBeatmapModel { ID = 2115170, BeatmapsetID = 1011020, Version = "Extra" };
        osu.Profiles[7405768] = new WebUserModel { ID = 7405768, Username = "Storyboarder" };
        osu.Profiles[3] = new WebUserModel { ID = 3, Username = "Carol" };
    }

    public void Dispose() => database.Dispose();

    private SubmissionService Service() => new(database.CreateContext(), osu, NullLogger<SubmissionService>.Instance);

    private static SubmitForm Form(string beatmapset = "https://osu.ppy.sh/beatmapsets/1011020", string storyboarders = "Storyboarder, osu.ppy.sh/users/3", string? video = null) => new()
    {
        Beatmapset = beatmapset,
        Storyboarders = storyboarders,
        Medium = "storybrew",
        Video = video,
        Tags = ["particles", "made-up"],
        Note = "  Watch the Extra difficulty.  ",
    };

    private async Task<ShowcaseSubmission> SavedAsync(int id)
    {
        await using var db = database.CreateContext();
        return await db.Submissions.Include(s => s.Storyboarders).Include(s => s.SuggestedTags).SingleAsync(s => s.Id == id);
    }

    [Fact]
    public async Task ASubmission_CopiesTheDetailsFromOsu_AndWaitsForReview()
    {
        var result = await Service().SubmitAsync(Form(video: "https://youtu.be/dQw4w9WgXcQ?si=share"), Bob);

        Assert.Empty(result.Errors);
        var saved = await SavedAsync(result.SubmissionId!.Value);
        Assert.Equal((1011020, "DYE/Re:flection+", "AVTechNO!xTreow", 6607303, "The Mapper"), (saved.BeatmapsetId, saved.Title, saved.Artist, saved.HostId, saved.HostUsername));
        Assert.Equal(new DateOnly(2019, 7, 30), saved.BeatmapSubmittedOn);
        Assert.True(saved.OsuListsStoryboard);
        Assert.Equal("Storybrew", saved.Medium);
        Assert.Equal("https://www.youtube.com/embed/dQw4w9WgXcQ", saved.VideoUrl);
        Assert.Equal("Watch the Extra difficulty.", saved.Note);
        Assert.Equal(["Storyboarder", "Carol"], saved.Credits.Select(c => c.Username));
        Assert.Equal([7405768, 3], saved.Credits.Select(c => c.OsuUserId));
        Assert.Equal(["particles"], saved.SuggestedTags.Select(t => t.Slug));
        Assert.Equal((2, "bob", SubmissionStatus.Pending, false), (saved.SubmitterId, saved.SubmitterUsername, saved.Status, saved.OutcomeSeen));
    }

    [Fact]
    public async Task ADifficultyLink_FindsItsBeatmapset()
    {
        var result = await Service().SubmitAsync(Form("https://osu.ppy.sh/b/2115170"), Bob);

        Assert.Equal(1011020, (await SavedAsync(result.SubmissionId!.Value)).BeatmapsetId);
    }

    public static TheoryData<string, string, string, string> Mistakes => new()
    {
        { "Blue Zenith", "Storyboarder", "Beatmapset", "Paste a link to the beatmapset, like https://osu.ppy.sh/beatmapsets/1011020, or its ID." },
        { "999", "Storyboarder", "Beatmapset", "osu! has no beatmapset 999." },
        { "https://osu.ppy.sh/b/888", "Storyboarder", "Beatmapset", "osu! has no beatmap 888." },
        { "https://osu.ppy.sh/beatmapsets/100", "Storyboarder", "Beatmapset", "This storyboard is already in the showcase." },
        { "1011020", "Ghost, Carol, Nobody", "Storyboarders", "osu! has no player called Ghost and Nobody." },
        { "1011020", " , ", "Storyboarders", "List at least one storyboarder." },
        { "1011020", string.Join(",", Enumerable.Range(1, 11).Select(i => $"Person {i}")), "Storyboarders", "List up to 10 storyboarders." },
    };

    [Theory]
    [MemberData(nameof(Mistakes))]
    public async Task Mistakes_AreExplainedOnTheirField_AndNothingIsSaved(string beatmapset, string storyboarders, string field, string message)
    {
        var result = await Service().SubmitAsync(Form(beatmapset, storyboarders), Bob);

        Assert.Null(result.SubmissionId);
        Assert.Equal(message, Assert.Contains(field, result.Errors));
        await using var db = database.CreateContext();
        Assert.Equal(0, await db.Submissions.CountAsync());
    }

    [Fact]
    public async Task AStoryboardAlreadyShowcased_IsLinked()
    {
        var result = await Service().SubmitAsync(Form("100"), Bob);

        Assert.Equal(100, result.ShowcasedId);
    }

    [Fact]
    public async Task AVideoLink_MustBeYouTube()
    {
        var result = await Service().SubmitAsync(Form(video: "https://vimeo.com/123456"), Bob);

        Assert.Equal("Use a YouTube link, like https://youtu.be/dQw4w9WgXcQ.", Assert.Contains("Video", result.Errors));
    }

    [Fact]
    public async Task AStoryboardWaitingForReview_CantBeSubmittedAgain()
    {
        await Service().SubmitAsync(Form(), Bob);

        var second = await Service().SubmitAsync(Form(), Carol);

        Assert.Equal("This storyboard is already waiting for review.", Assert.Contains("Beatmapset", second.Errors));
    }

    [Fact]
    public async Task WhenOsuCantBeReached_TheMemberIsToldToTryAgain()
    {
        osu.Unreachable = true;

        var result = await Service().SubmitAsync(Form(), Bob);

        Assert.Equal("Couldn't reach osu! Try again in a few minutes.", Assert.Contains("", result.Errors));
    }

    [Fact]
    public async Task FiveSubmissionsWaiting_IsTheLimit()
    {
        var service = Service();
        await using (var db = database.CreateContext())
        {
            for (int i = 0; i < SubmissionService.MaxWaitingPerMember; i++)
                db.Submissions.Add(new ShowcaseSubmission { BeatmapsetId = 500 + i, Title = "t", Artist = "a", HostUsername = "h", Medium = "m", SubmitterId = Bob.Id, SubmitterUsername = Bob.Username });
            await db.SaveChangesAsync();
        }

        var result = await service.SubmitAsync(Form(), Bob);
        var someoneElse = await service.SubmitAsync(Form(), Carol);

        Assert.Equal("You have 5 submissions waiting for review. You can add more once they're reviewed.", Assert.Contains("", result.Errors));
        Assert.NotNull(someoneElse.SubmissionId);
    }

    [Fact]
    public async Task TheSamePersonTwice_IsCreditedOnce()
    {
        var result = await Service().SubmitAsync(Form(storyboarders: "Carol, 3, https://osu.ppy.sh/users/3"), Bob);

        Assert.Equal(["Carol"], (await SavedAsync(result.SubmissionId!.Value)).Credits.Select(c => c.Username));
    }

    [Fact]
    public async Task ABeatmapsetOsuListsNoStoryboardFor_IsStillAccepted()
    {
        var result = await Service().SubmitAsync(Form("292301"), Bob);

        Assert.False((await SavedAsync(result.SubmissionId!.Value)).OsuListsStoryboard);
    }

    [Fact]
    public async Task RemovedOrDeclinedStoryboards_CanBeSubmittedAgain()
    {
        await using (var db = database.CreateContext())
        {
            db.ShowcaseRemovals.Add(new ShowcaseRemoval { BeatmapsetId = 1011020, RemovedAt = DateTime.UtcNow, RemovedByUsername = "reviewer", Reason = "Video gone." });
            db.Submissions.Add(new ShowcaseSubmission { BeatmapsetId = 292301, Title = "t", Artist = "a", HostUsername = "h", Medium = "m", SubmitterId = Carol.Id, Status = SubmissionStatus.Declined });
            await db.SaveChangesAsync();
        }

        Assert.NotNull((await Service().SubmitAsync(Form("1011020"), Bob)).SubmissionId);
        Assert.NotNull((await Service().SubmitAsync(Form("292301"), Bob)).SubmissionId);
    }

    [Fact]
    public async Task MySubmissions_AreOnlyMine_NewestFirst()
    {
        await Service().SubmitAsync(Form("1011020"), Bob);
        await Service().SubmitAsync(Form("292301"), Bob);
        await Service().SubmitAsync(Form("409681"), Carol);

        Assert.Equal([292301, 1011020], (await Service().GetMineAsync(Bob.Id)).Select(s => s.BeatmapsetId));
    }

    [Fact]
    public async Task AMember_WithdrawsTheirOwnWaitingSubmission_Once()
    {
        int id = (await Service().SubmitAsync(Form(), Bob)).SubmissionId!.Value;

        Assert.Equal(WithdrawOutcome.NotFound, await Service().WithdrawAsync(id, Carol.Id));
        Assert.Equal(WithdrawOutcome.Withdrawn, await Service().WithdrawAsync(id, Bob.Id));
        Assert.Equal(WithdrawOutcome.AlreadyReviewed, await Service().WithdrawAsync(id, Bob.Id));
        Assert.Equal(WithdrawOutcome.NotFound, await Service().WithdrawAsync(id + 100, Bob.Id));

        var withdrawn = await SavedAsync(id);
        Assert.Equal(SubmissionStatus.Withdrawn, withdrawn.Status);
        Assert.NotNull(withdrawn.ReviewedAt);
        Assert.True(withdrawn.OutcomeSeen);
    }

    [Fact]
    public async Task AReviewedSubmission_ShowsTheDot_UntilTheMemberLooks()
    {
        int id = (await Service().SubmitAsync(Form(), Bob)).SubmissionId!.Value;
        Assert.False(await Service().HasUnseenOutcomesAsync(Bob.Id), "a waiting submission has no news");

        await using (var db = database.CreateContext())
            await db.Submissions.Where(s => s.Id == id).ExecuteUpdateAsync(set => set.SetProperty(s => s.Status, SubmissionStatus.Declined));
        Assert.True(await Service().HasUnseenOutcomesAsync(Bob.Id));
        Assert.False(await Service().HasUnseenOutcomesAsync(Carol.Id));

        await Service().MarkOutcomesSeenAsync(Bob.Id);
        Assert.False(await Service().HasUnseenOutcomesAsync(Bob.Id));
    }

    [Fact]
    public async Task ApprovedSubmissions_KnowWhetherTheyreStillShowcased()
    {
        var showcased = await Service().ShowcasedAsync([100, 1011020, 100]);

        Assert.Equal([100], showcased);
    }
}
