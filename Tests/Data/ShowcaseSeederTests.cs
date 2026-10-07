using System.Text;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Tests.Infrastructure;

namespace osb.Tests.Data;

public class ShowcaseSeederTests
{
    [Fact]
    public async Task AnEmptyDatabase_GetsEverythingInTheFile()
    {
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
            await ShowcaseSeeder.SeedAsync(db, CancellationToken.None);

        var seed = await ShowcaseSeeder.LoadAsync(CancellationToken.None);
        await using var check = database.CreateContext();
        Assert.Equal(seed.Beatmapsets.Count, await check.Beatmapsets.CountAsync());
        Assert.Equal(seed.Users.Count, await check.Users.CountAsync());
        Assert.Equal(seed.Roles.Count, await check.Roles.CountAsync());
        Assert.Equal(seed.Tags.Count, await check.Tags.CountAsync());
        Assert.Equal(seed.Beatmapsets.Sum(s => s.Storyboarders.Count), await check.BeatmapsetStoryboarders.CountAsync());
        Assert.Equal(seed.Beatmapsets.Sum(s => s.Tags.Count), await check.Set<Dictionary<string, object>>("BeatmapsetTags").CountAsync());
        Assert.Equal(seed.Users.Count(u => u.CommunityMember), await check.Users.CountAsync(u => u.IsCommunityMember));
        Assert.Equal(seed.Beatmapsets.Count(s => s.Video != null), await check.Beatmapsets.CountAsync(s => s.VideoUrl != null));
    }

    [Fact]
    public async Task EachStoryboard_IsCopiedAsWritten_WithCreditsInOrder()
    {
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
            await ShowcaseSeeder.SeedAsync(db, CancellationToken.None);

        var seed = await ShowcaseSeeder.LoadAsync(CancellationToken.None);
        var expected = seed.Beatmapsets.First(s => s.Storyboarders.Count > 1 && s.Tags.Count > 0);
        await using var check = database.CreateContext();
        var set = await check.Beatmapsets
            .Include(s => s.Host).Include(s => s.Tags).Include(s => s.Storyboarders)
            .SingleAsync(s => s.Id == expected.Id);

        Assert.Equal(expected.Title, set.Title);
        Assert.Equal(expected.Artist, set.Artist);
        Assert.Equal(expected.Host, set.HostId);
        Assert.Equal(expected.Medium, set.Medium);
        Assert.Equal(expected.Submitted, set.SubmittedOn);
        Assert.Equal(expected.Showcased, set.ShowcasedOn);
        Assert.Equal(expected.Video, set.VideoUrl);
        Assert.Equal(expected.Tags.Order(), set.Tags.Select(t => t.Slug).Order());
        Assert.Equal(expected.Storyboarders, set.Storyboarders.OrderBy(c => c.Position).Select(c => c.OsuUserId));
        Assert.Equal(Enumerable.Range(0, expected.Storyboarders.Count), set.Storyboarders.Select(c => c.Position).Order());
    }

    [Fact]
    public async Task ASecondStart_AddsNothing_AndKeepsEditsMadeInTheDatabase()
    {
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
            await ShowcaseSeeder.SeedAsync(db, CancellationToken.None);

        int id;
        await using (var db = database.CreateContext())
        {
            var set = await db.Beatmapsets.OrderBy(s => s.Id).FirstAsync();
            id = set.Id;
            set.Title = "Edited on the site";
            (await db.Users.FirstAsync()).Username = "renamed";
            await db.SaveChangesAsync();
        }

        await using (var db = database.CreateContext())
            await ShowcaseSeeder.SeedAsync(db, CancellationToken.None);

        var seed = await ShowcaseSeeder.LoadAsync(CancellationToken.None);
        await using var check = database.CreateContext();
        Assert.Equal(seed.Beatmapsets.Count, await check.Beatmapsets.CountAsync());
        Assert.Equal(seed.Users.Count, await check.Users.CountAsync());
        Assert.Equal(seed.Beatmapsets.Sum(s => s.Storyboarders.Count), await check.BeatmapsetStoryboarders.CountAsync());
        Assert.Equal("Edited on the site", (await check.Beatmapsets.FindAsync(id))!.Title);
        Assert.Equal(1, await check.Users.CountAsync(u => u.Username == "renamed"));
    }

    [Fact]
    public async Task OnlyMissingVideos_AreFilledIn()
    {
        var seed = await ShowcaseSeeder.LoadAsync(CancellationToken.None);
        var withVideos = seed.Beatmapsets.Where(s => s.Video != null).Take(2).ToList();
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
            await ShowcaseSeeder.SeedAsync(db, CancellationToken.None);

        await using (var db = database.CreateContext())
        {
            (await db.Beatmapsets.FindAsync(withVideos[0].Id))!.VideoUrl = null;
            (await db.Beatmapsets.FindAsync(withVideos[1].Id))!.VideoUrl = "https://www.youtube.com/embed/aaaaaaaaaaa";
            await db.SaveChangesAsync();
        }

        await using (var db = database.CreateContext())
            await ShowcaseSeeder.SeedAsync(db, CancellationToken.None);

        await using var check = database.CreateContext();
        Assert.Equal(withVideos[0].Video, (await check.Beatmapsets.FindAsync(withVideos[0].Id))!.VideoUrl);
        Assert.Equal("https://www.youtube.com/embed/aaaaaaaaaaa", (await check.Beatmapsets.FindAsync(withVideos[1].Id))!.VideoUrl);
    }

    [Theory]
    [InlineData("\"storyboarders\": [2]", "\"storyboarders\": [99]", "unknown storyboarder 99 of beatmapset 1")]
    [InlineData("\"host\": 2", "\"host\": 99", "unknown host 99 of beatmapset 1")]
    [InlineData("\"tags\": [\"particles\"]", "\"tags\": [\"sparkles\"]", "unknown tag 'sparkles' of beatmapset 1")]
    [InlineData("\"roles\": [\"Mentor\"]", "\"roles\": [\"Wizard\"]", "unknown role 'Wizard' of user 2")]
    public async Task AFileWithAMistake_StopsTheStart_NamingTheEntry_AndAddsNothing(string valid, string mistake, string message)
    {
        string json = ValidFile.Replace(valid, mistake);
        Assert.NotEqual(ValidFile, json);
        using var database = new TestDatabase();

        await using (var db = database.CreateContext())
        {
            var error = await Assert.ThrowsAsync<InvalidDataException>(() => SeedFromAsync(db, json));
            Assert.Equal($"showcase.json: {message}. Add it to the file first.", error.Message);
        }

        await using var check = database.CreateContext();
        Assert.Equal(0, await check.Beatmapsets.CountAsync());
        Assert.Equal(0, await check.Users.CountAsync());
    }

    [Fact]
    public async Task TheSmallestValidFile_Loads()
    {
        using var database = new TestDatabase();
        await using (var db = database.CreateContext())
            await SeedFromAsync(db, ValidFile);

        await using var check = database.CreateContext();
        var set = await check.Beatmapsets.Include(s => s.Storyboarders).ThenInclude(c => c.User).SingleAsync();
        Assert.Equal("bob", set.Credits.Single().Username);
    }

    [Fact]
    public async Task AnEmptyFile_IsRefused()
    {
        using var database = new TestDatabase();
        await using var db = database.CreateContext();

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => SeedFromAsync(db, "null"));
        Assert.Equal("showcase.json is empty.", error.Message);
    }

    private static Task SeedFromAsync(OsbDbContext db, string json) =>
        ShowcaseSeeder.SeedAsync(db, new MemoryStream(Encoding.UTF8.GetBytes(json)), CancellationToken.None);

    private const string ValidFile = """
        {
          "roles": [{ "name": "Mentor", "colour": "#2ecc71", "rank": 6 }],
          "tags": [{ "slug": "particles", "name": "Particles", "rating": 15 }],
          "users": [
            { "id": 2, "username": "bob", "communityMember": true, "roles": ["Mentor"] }
          ],
          "beatmapsets": [
            {
              "id": 1, "title": "Song", "artist": "Artist", "host": 2, "medium": "Storybrew",
              "submitted": "2020-01-01", "showcased": "2020-02-01",
              "storyboarders": [2], "tags": ["particles"], "video": null
            }
          ]
        }
        """;
}
