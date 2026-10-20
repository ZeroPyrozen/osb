using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using osb.Data;
using osb.Helpers;
using osb.Services;
using osb.Tests.Infrastructure;
using osb.ViewModels;

namespace osb.Tests.Services;

/// <summary>Reviewers correcting and removing showcased storyboards, against a small showcase and the fake osu!.</summary>
public sealed class ShowcaseEditTests : IDisposable
{
    private static readonly Member Reviewer = new(9001, "Reviewer");

    private readonly TestDatabase database = new();
    private readonly FakeOsu osu = new();

    /// <summary>
    /// Beatmapset 100, Artist - Song: hosted by Host, storyboarded by Dave (a community member) and Ann,
    /// tagged Particles and Lyrics, with a video. The 3D tag is free to add.
    /// </summary>
    public ShowcaseEditTests()
    {
        using var db = database.CreateContext();
        db.Tags.Add(new StoryboardTag { Slug = "3d", Name = "3D", Rating = 30 });
        db.Beatmapsets.Add(new Beatmapset
        {
            Id = 100, Title = "Song", Artist = "Artist", Host = new OsuUser { Id = 5, Username = "Host" }, Medium = "Storybrew",
            SubmittedOn = new DateOnly(2020, 1, 1), ShowcasedOn = new DateOnly(2020, 2, 1), VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            Tags =
            [
                new StoryboardTag { Slug = "particles", Name = "Particles", Rating = 15 },
                new StoryboardTag { Slug = "lyrics", Name = "Lyrics", Rating = 10 },
            ],
            Storyboarders =
            [
                new BeatmapsetStoryboarder { User = new OsuUser { Id = 4, Username = "Dave", IsCommunityMember = true }, Position = 0 },
                new BeatmapsetStoryboarder { User = new OsuUser { Id = 6, Username = "Ann" }, Position = 1 },
            ],
        });
        db.SaveChanges();

        osu.Profiles[8001] = new WebUserModel { ID = 8001, Username = "NewPerson" };
    }

    public void Dispose() => database.Dispose();

    private ReviewService Service() => new(database.CreateContext(), osu, NullLogger<ReviewService>.Instance);

    /// <summary>The storyboard as its page shows it, or null when it's not in the showcase.</summary>
    private Task<Beatmapset?> ShowcasedAsync() => new ShowcaseService(database.CreateContext()).GetAsync(100);

    private async Task<EntryForm> FormAsync() => ReviewService.FormFor((await ShowcasedAsync())!);

    /// <summary>Loads a showcase.json that lists the storyboard as it first was, video included.</summary>
    private async Task LoadShowcaseJsonAsync()
    {
        const string json = """
            {
              "roles": [],
              "tags": [{ "slug": "particles", "name": "Particles", "rating": 15 }],
              "users": [{ "id": 5, "username": "Host", "communityMember": false, "roles": [] }],
              "beatmapsets": [
                {
                  "id": 100, "title": "Song", "artist": "Artist", "host": 5, "medium": "Storybrew",
                  "submitted": "2020-01-01", "showcased": "2020-02-01",
                  "storyboarders": [5], "tags": ["particles"], "video": "https://www.youtube.com/embed/dQw4w9WgXcQ"
                }
              ]
            }
            """;
        await using var db = database.CreateContext();
        await ShowcaseSeeder.SeedAsync(db, new MemoryStream(Encoding.UTF8.GetBytes(json)), CancellationToken.None);
    }

    [Fact]
    public async Task TheForm_StartsFromTheStoryboardAsItIs()
    {
        var form = await FormAsync();

        Assert.Equal(("Song", "Artist", "Host", "Dave, Ann", "Storybrew"), (form.Title, form.Artist, form.Mapper, form.Storyboarders, form.Medium));
        Assert.Equal("https://www.youtube.com/embed/dQw4w9WgXcQ", form.Video);
        Assert.Equal(["lyrics", "particles"], form.Tags.Order());
        Assert.Equal((new DateOnly(2020, 1, 1), new DateOnly(2020, 2, 1)), (form.SubmittedOn, form.ShowcasedOn));
    }

    [Fact]
    public async Task Saving_ChangesWhatThePageShows_AndMarksItChangedOnTheSite()
    {
        var form = await FormAsync();
        form.Title = " Song (TV Size) ";
        form.Artist = "Other Artist";
        form.Mapper = "Dave";
        form.Storyboarders = "Ann, NewPerson";
        form.Medium = "storybrew";
        form.Video = "";
        form.Tags = ["particles", "3d"];
        form.SubmittedOn = new DateOnly(2019, 12, 31);
        form.ShowcasedOn = new DateOnly(2026, 10, 7);

        var result = await Service().EditAsync(100, form);

        Assert.Equal(ReviewOutcome.Saved, result.Outcome);
        var set = (await ShowcasedAsync())!;
        Assert.Equal(("Song (TV Size)", "Other Artist", "Storybrew"), (set.Title, set.Artist, set.Medium));
        Assert.Equal("Dave", set.Host.Username);
        Assert.Equal(["Ann", "NewPerson"], set.Credits.Select(u => u.Username));
        Assert.Null(set.VideoUrl);
        Assert.Equal(45, set.OsbLevel);
        Assert.Equal((new DateOnly(2019, 12, 31), new DateOnly(2026, 10, 7)), (set.SubmittedOn, set.ShowcasedOn));
        Assert.NotNull(set.ChangedOnSiteAt);
    }

    [Fact]
    public async Task Saving_WorksWhileOsuIsDown_AsLongAsNobodyIsNew()
    {
        osu.Unreachable = true;
        var form = await FormAsync();
        form.Storyboarders = "Ann, dave";

        Assert.Equal(ReviewOutcome.Saved, (await Service().EditAsync(100, form)).Outcome);
        Assert.Equal(["Ann", "Dave"], (await ShowcasedAsync())!.Credits.Select(u => u.Username));
    }

    [Fact]
    public async Task Problems_AreExplainedOnTheirField_AndNothingChanges()
    {
        var form = await FormAsync();
        form.Title = "Changed";
        form.Storyboarders = "Dave, Ghost";
        form.Video = "https://vimeo.com/1";

        var result = await Service().EditAsync(100, form);

        Assert.Equal(ReviewOutcome.Invalid, result.Outcome);
        Assert.Equal("osu! has no player called Ghost.", Assert.Contains("Storyboarders", result.Errors));
        Assert.Equal("Use a YouTube link, like https://youtu.be/dQw4w9WgXcQ.", Assert.Contains("Video", result.Errors));
        var set = (await ShowcasedAsync())!;
        Assert.Equal(("Song", null), (set.Title, set.ChangedOnSiteAt));
    }

    [Fact]
    public async Task AStoryboardThatIsntShowcased_CantBeEdited() =>
        Assert.Equal(ReviewOutcome.NotFound, (await Service().EditAsync(101, await FormAsync())).Outcome);

    [Fact]
    public async Task AnEdit_Stays_WhenShowcaseJsonIsLoadedAgain()
    {
        var form = await FormAsync();
        form.Video = "";
        await Service().EditAsync(100, form);

        await LoadShowcaseJsonAsync();

        Assert.Null((await ShowcasedAsync())!.VideoUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Removing_NeedsAReason(string? reason)
    {
        var result = await Service().RemoveAsync(100, reason, Reviewer);

        Assert.Equal("Give a reason for removing it.", Assert.Contains("Reason", result.Errors));
        Assert.NotNull(await ShowcasedAsync());
        Assert.Null(await Service().GetRemovalAsync(100));
    }

    [Fact]
    public async Task AReason_IsUpTo1000Characters()
    {
        var tooLong = await Service().RemoveAsync(100, new string('a', 1001), Reviewer);
        var longest = await Service().RemoveAsync(100, new string('a', 1000), Reviewer);

        Assert.Equal("Keep the reason to 1,000 characters.", Assert.Contains("Reason", tooLong.Errors));
        Assert.Equal(ReviewOutcome.Removed, longest.Outcome);
    }

    [Fact]
    public async Task Removing_TakesItOutOfTheShowcase_AndRemembersWhoAndWhy()
    {
        var result = await Service().RemoveAsync(100, "  The video is gone.  ", Reviewer);

        Assert.Equal(ReviewOutcome.Removed, result.Outcome);
        Assert.Null(await ShowcasedAsync());
        var removal = (await Service().GetRemovalAsync(100))!;
        Assert.Equal((9001, "Reviewer", "The video is gone."), (removal.RemovedById, removal.RemovedByUsername, removal.Reason));
        Assert.True(DateTime.UtcNow - removal.RemovedAt < TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ItsPeopleAndTags_StayOnTheSite()
    {
        await Service().RemoveAsync(100, "A duplicate.", Reviewer);

        await using var db = database.CreateContext();
        Assert.Equal([4, 5, 6], await db.Users.Select(u => u.Id).OrderBy(id => id).ToListAsync());
        Assert.True((await db.Users.SingleAsync(u => u.Id == 4)).IsCommunityMember);
        Assert.Equal(3, await db.Tags.CountAsync());
        Assert.False(await db.BeatmapsetStoryboarders.AnyAsync());
        Assert.False(await db.Tags.AnyAsync(t => t.Beatmapsets.Any()));
    }

    [Fact]
    public async Task ARemovedStoryboard_StaysGone_WhenShowcaseJsonIsLoadedAgain()
    {
        await Service().RemoveAsync(100, "A duplicate.", Reviewer);

        await LoadShowcaseJsonAsync();

        Assert.Null(await ShowcasedAsync());
    }

    [Fact]
    public async Task RemovingItAgain_FindsNothing_AndKeepsTheFirstReason()
    {
        await Service().RemoveAsync(100, "A duplicate.", Reviewer);

        var again = await Service().RemoveAsync(100, "Some other reason.", Reviewer);

        Assert.Equal(ReviewOutcome.NotFound, again.Outcome);
        Assert.Equal("A duplicate.", (await Service().GetRemovalAsync(100))!.Reason);
    }
}
