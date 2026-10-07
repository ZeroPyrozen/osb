using osb.Data;
using osb.Services;
using osb.Tests.Infrastructure;

namespace osb.Tests.Services;

/// <summary>A small showcase, shared by the tests below. They only read it.</summary>
public sealed class SmallShowcase : IDisposable
{
    public TestDatabase Database { get; } = new();

    public SmallShowcase()
    {
        using var db = Database.CreateContext();
        var mentor = new CommunityRole { Name = "Mentor", Colour = "#2ecc71", Rank = 6 };
        var storyboarder = new CommunityRole { Name = "Storyboarder", Colour = "#f1c40f", Rank = 2 };
        var verified = new CommunityRole { Name = "Verified", Colour = "#90a4b0", Rank = 0 };

        var alice = new OsuUser { Id = 1, Username = "Alice", IsCommunityMember = true, Roles = [mentor, verified] };
        var bob = new OsuUser { Id = 2, Username = "bob", IsCommunityMember = true, Roles = [storyboarder] };
        var carol = new OsuUser { Id = 3, Username = "Carol" };
        var dave = new OsuUser { Id = 4, Username = "Dave" };
        var eve = new OsuUser { Id = 5, Username = "eve", IsCommunityMember = true, Roles = [verified] };
        var zed = new OsuUser { Id = 6, Username = "Zed", IsCommunityMember = true };
        var adam = new OsuUser { Id = 7, Username = "adam", IsCommunityMember = true };

        var particles = new StoryboardTag { Slug = "particles", Name = "Particles", Rating = 15 };
        var lyrics = new StoryboardTag { Slug = "lyrics", Name = "Lyrics", Rating = 10 };
        var threeD = new StoryboardTag { Slug = "3d", Name = "3D", Rating = 30 };

        db.Users.AddRange(alice, bob, carol, dave, eve, zed, adam);
        db.Beatmapsets.AddRange(
            Set(100, "Blue Zenith", "xi", dave, "Storybrew", new DateOnly(2024, 1, 10), "https://www.youtube.com/embed/aaaaaaaaaaa", [alice], [particles]),
            Set(101, "100% Orange Juice", "Hanyuu", alice, "SGL", new DateOnly(2024, 2, 1), "https://www.youtube.com/embed/bbbbbbbbbbb", [bob, alice], [lyrics, threeD]),
            Set(102, "a_b test", "Foo", dave, "Storybrew", new DateOnly(2024, 2, 1), null, [carol], []),
            Set(103, "aXb", "Bar", bob, "C#", new DateOnly(2023, 5, 5), null, [bob], [threeD]));
        db.SaveChanges();
    }

    private static Beatmapset Set(int id, string title, string artist, OsuUser host, string medium, DateOnly showcased, string? video, OsuUser[] credits, StoryboardTag[] tags) => new()
    {
        Id = id,
        Title = title,
        Artist = artist,
        Host = host,
        Medium = medium,
        SubmittedOn = showcased.AddMonths(-6),
        ShowcasedOn = showcased,
        VideoUrl = video,
        Storyboarders = credits.Select((user, position) => new BeatmapsetStoryboarder { User = user, Position = position }).ToList(),
        Tags = tags.ToList(),
    };

    public void Dispose() => Database.Dispose();
}

public class ShowcaseServiceTests(SmallShowcase showcase) : IClassFixture<SmallShowcase>
{
    private ShowcaseService Service() => new(showcase.Database.CreateContext());

    private async Task<int[]> Search(string? query = null, string? tag = null, string? medium = null) =>
        (await Service().SearchAsync(query, tag, medium, page: 1, pageSize: 50)).Items.Select(s => s.Id).ToArray();

    [Theory]
    [InlineData("blue", new[] { 100 })]
    [InlineData("HANYUU", new[] { 101 })]
    [InlineData("dave", new[] { 102, 100 })]
    [InlineData("carol", new[] { 102 })]
    [InlineData("  blue  ", new[] { 100 })]
    public async Task Search_MatchesTitleArtistMapperAndStoryboarder_IgnoringCase(string query, int[] expected) =>
        Assert.Equal(expected, await Search(query));

    [Theory]
    [InlineData("100%", new[] { 101 })]
    [InlineData("o%J", new int[0])]
    [InlineData("a_b", new[] { 102 })]
    public async Task Search_TakesPercentAndUnderscoreLiterally(string query, int[] expected) =>
        Assert.Equal(expected, await Search(query));

    [Fact]
    public async Task Filters_Combine()
    {
        Assert.Equal(new[] { 101, 103 }, await Search(tag: "3d"));
        Assert.Equal(new[] { 102, 100 }, await Search(medium: "Storybrew"));
        Assert.Equal(new[] { 103 }, await Search(tag: "3d", medium: "C#"));
        Assert.Equal(new[] { 103 }, await Search("bar", tag: "3d"));
        Assert.Empty(await Search(tag: "no-such-tag"));
    }

    [Fact]
    public async Task Results_AreNewestFirst_ThenByHighestId() =>
        Assert.Equal(new[] { 102, 101, 100, 103 }, await Search());

    [Theory]
    [InlineData(1, 1, new[] { 102, 101 })]
    [InlineData(2, 2, new[] { 100, 103 })]
    [InlineData(0, 1, new[] { 102, 101 })]
    [InlineData(9, 2, new[] { 100, 103 })]
    public async Task Pages_StayWithinTheResults(int requested, int shown, int[] expected)
    {
        var page = await Service().SearchAsync(null, null, null, requested, pageSize: 2);

        Assert.Equal(shown, page.Page);
        Assert.Equal(expected, page.Items.Select(s => s.Id));
        Assert.Equal(4, page.Total);
        Assert.Equal(2, page.PageCount);
    }

    [Fact]
    public async Task NoResults_IsStillPageOneOfOne()
    {
        var page = await Service().SearchAsync("nothing matches this", null, null, 3, pageSize: 2);

        Assert.Empty(page.Items);
        Assert.Equal(1, page.Page);
        Assert.Equal(1, page.PageCount);
    }

    [Fact]
    public async Task AStoryboard_ComesWithItsMapperTagsAndCreditsInOrder()
    {
        var set = (await Service().GetAsync(101))!;

        Assert.Equal("Alice", set.Host.Username);
        Assert.Equal(["3d", "lyrics"], set.Tags.Select(t => t.Slug).Order());
        Assert.Equal(["bob", "Alice"], set.Credits.Select(u => u.Username));
        Assert.Equal(40, set.OsbLevel);
    }

    [Fact]
    public async Task AnUnknownStoryboard_IsNull() =>
        Assert.Null(await Service().GetAsync(999));

    [Fact]
    public async Task Related_AreOtherStoryboardsByTheSamePeople_NewestFirst()
    {
        var service = Service();
        var set = (await service.GetAsync(101))!;

        Assert.Equal([100, 103], (await service.GetRelatedAsync(set, 4)).Select(s => s.Id));
        Assert.Equal([100], (await service.GetRelatedAsync(set, 1)).Select(s => s.Id));
    }

    /// <summary>
    /// The home page's "Now playing" used to show credits from a different storyboard: the split
    /// queries ran the random ordering once for the storyboard and again for its credits.
    /// </summary>
    [Fact]
    public async Task ARandomStoryboardWithAVideo_ComesWithItsOwnCredits()
    {
        var credits = new Dictionary<int, string[]> { [100] = ["Alice"], [101] = ["bob", "Alice"] };

        for (int i = 0; i < 30; i++)
        {
            var set = (await Service().GetRandomAsync(withVideo: true))!;

            Assert.NotNull(set.VideoUrl);
            Assert.Equal(credits[set.Id], set.Credits.Select(u => u.Username));
        }
    }

    [Fact]
    public async Task ARandomStoryboard_IsNullWhenThereAreNone()
    {
        using var empty = new TestDatabase();

        Assert.Null(await new ShowcaseService(empty.CreateContext()).GetRandomAsync(withVideo: false));
    }

    [Fact]
    public async Task TheCommunity_IsMembersOnly_HighestRoleFirst_ThenByName() =>
        Assert.Equal(["Alice", "bob", "eve", "adam", "Zed"], (await Service().GetCommunityAsync()).Select(u => u.Username));

    [Fact]
    public async Task AProfile_ListsThePersonsStoryboards_NewestFirst()
    {
        var service = Service();

        Assert.Equal([101, 100], (await service.GetStoryboarderAsync(1))!.Storyboards.Select(s => s.Id));
        Assert.Equal([102], (await service.GetStoryboarderAsync(3))!.Storyboards.Select(s => s.Id));
        Assert.Empty((await service.GetStoryboarderAsync(5))!.Storyboards);
    }

    [Fact]
    public async Task AProfile_IsNullForUnknownPeople_AndMappersWhoNeverStoryboarded()
    {
        var service = Service();

        Assert.Null(await service.GetStoryboarderAsync(999));
        Assert.Null(await service.GetStoryboarderAsync(4));
    }

    [Fact]
    public async Task TheHomePage_CountsStoryboardsPeopleAndTools()
    {
        var summary = await Service().GetHomeSummaryAsync(recentCount: 2);

        Assert.Equal(4, summary.StoryboardCount);
        Assert.Equal(3, summary.StoryboarderCount);
        Assert.Equal([new MediumCount("Storybrew", 2), new MediumCount("C#", 1), new MediumCount("SGL", 1)], summary.TopMediums);
        Assert.Equal([102, 101], summary.Recent.Select(s => s.Id));
        Assert.NotNull(summary.FeaturedVideo?.VideoUrl);
    }

    [Fact]
    public async Task TheHomePage_ShowsTheFiveMostUsedTools()
    {
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
        {
            var host = new OsuUser { Id = 1, Username = "host" };
            string[] mediums = ["A", "B", "B", "C", "D", "E", "F", "F", "F"];
            db.Beatmapsets.AddRange(mediums.Select((medium, i) => new Beatmapset
            {
                Id = i + 1, Title = $"Song {i}", Artist = "Artist", Host = host, Medium = medium,
                SubmittedOn = new DateOnly(2020, 1, 1), ShowcasedOn = new DateOnly(2020, 1, 1),
            }));
            await db.SaveChangesAsync();
        }

        var summary = await new ShowcaseService(database.CreateContext()).GetHomeSummaryAsync(recentCount: 1);

        Assert.Equal(["F", "B", "A", "C", "D"], summary.TopMediums.Select(m => m.Medium));
    }

    [Fact]
    public async Task TagsAndTools_ForTheFilters_AreSorted()
    {
        var service = Service();

        Assert.Equal(["3D", "Lyrics", "Particles"], (await service.GetTagsAsync()).Select(t => t.Name));
        Assert.Equal(["C#", "SGL", "Storybrew"], await service.GetMediumsAsync());
    }
}
