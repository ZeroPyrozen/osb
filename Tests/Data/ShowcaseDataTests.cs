using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.Tests.Infrastructure;

namespace osb.Tests.Data;

/// <summary>
/// Rules for Data/Seed/showcase.json. A mistake there stops the site from starting, so these catch it
/// in CI instead of in a deploy's health check.
/// </summary>
public partial class ShowcaseDataTests
{
    private static readonly ShowcaseSeeder.SeedFile Seed = ShowcaseSeeder.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>Beatmapsets osb.moe showcased that have since been deleted from osu!.</summary>
    private static readonly int[] DeletedFromOsu = [1152676, 571969, 554417, 535426, 511171, 473391];

    [Fact]
    public void Ids_AreUnique()
    {
        Assert.Empty(Duplicates(Seed.Beatmapsets.Select(s => s.Id)));
        Assert.Empty(Duplicates(Seed.Users.Select(u => u.Id)));
        Assert.Empty(Duplicates(Seed.Roles.Select(r => r.Name)));
        Assert.Empty(Duplicates(Seed.Tags.Select(t => t.Slug)));
    }

    [Fact]
    public void EveryMapperStoryboarderTagAndRole_IsInTheFile()
    {
        var users = Seed.Users.Select(u => u.Id).ToHashSet();
        var tags = Seed.Tags.Select(t => t.Slug).ToHashSet();
        var roles = Seed.Roles.Select(r => r.Name).ToHashSet();

        var missing = Seed.Beatmapsets.SelectMany(s =>
                (users.Contains(s.Host) ? [] : new[] { $"host {s.Host} of {s.Id}" })
                .Concat(s.Storyboarders.Where(id => !users.Contains(id)).Select(id => $"storyboarder {id} of {s.Id}"))
                .Concat(s.Tags.Where(slug => !tags.Contains(slug)).Select(slug => $"tag '{slug}' of {s.Id}")))
            .Concat(Seed.Users.SelectMany(u => u.Roles.Where(r => !roles.Contains(r)).Select(r => $"role '{r}' of user {u.Id}")))
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void EveryStoryboard_CreditsSomeone_EachPersonOnce() =>
        Assert.All(Seed.Beatmapsets, s =>
        {
            Assert.NotEmpty(s.Storyboarders);
            Assert.Equal(s.Storyboarders.Count, s.Storyboarders.Distinct().Count());
        });

    [Fact]
    public void Videos_AreYouTubeEmbedLinks() =>
        Assert.All(Seed.Beatmapsets.Where(s => s.Video != null), s =>
        {
            Assert.Matches(EmbedLink(), s.Video!);
            Assert.NotNull(Format.YouTubeId(s.Video));
        });

    [Fact]
    public void Text_FitsTheDatabaseColumns()
    {
        using var db = new OsbDbContext(new DbContextOptionsBuilder<OsbDbContext>().UseSqlite("Data Source=:memory:").Options);
        int Max<T>(string property) => db.Model.FindEntityType(typeof(T))!.FindProperty(property)!.GetMaxLength()!.Value;

        Assert.All(Seed.Beatmapsets, s =>
        {
            Assert.InRange(s.Title.Length, 1, Max<Beatmapset>(nameof(Beatmapset.Title)));
            Assert.InRange(s.Artist.Length, 1, Max<Beatmapset>(nameof(Beatmapset.Artist)));
            Assert.InRange(s.Medium.Length, 1, Max<Beatmapset>(nameof(Beatmapset.Medium)));
            Assert.InRange(s.Video?.Length ?? 0, 0, Max<Beatmapset>(nameof(Beatmapset.VideoUrl)));
        });
        Assert.All(Seed.Users, u => Assert.InRange(u.Username.Length, 1, Max<OsuUser>(nameof(OsuUser.Username))));
        Assert.All(Seed.Roles, r => Assert.InRange(r.Name.Length, 1, Max<CommunityRole>(nameof(CommunityRole.Name))));
        Assert.All(Seed.Tags, t =>
        {
            Assert.InRange(t.Slug.Length, 1, Max<StoryboardTag>(nameof(StoryboardTag.Slug)));
            Assert.InRange(t.Name.Length, 1, Max<StoryboardTag>(nameof(StoryboardTag.Name)));
        });
    }

    [Fact]
    public void Roles_HaveCssColours() =>
        Assert.All(Seed.Roles, r => Assert.Matches(HexColour(), r.Colour));

    [Fact]
    public void OnlyCommunityMembers_HaveRoles() =>
        Assert.All(Seed.Users.Where(u => !u.CommunityMember), u => Assert.Empty(u.Roles));

    [Fact]
    public void EveryTag_HasAnIcon()
    {
        string sprite = File.ReadAllText(Path.Combine(RepoPaths.WebRoot, "images", "showcase", "tags.svg"));

        Assert.All(Seed.Tags, t => Assert.Contains($"id=\"{t.Slug}\"", sprite));
    }

    [Fact]
    public void BeatmapsetsDeletedFromOsu_StayOut() =>
        Assert.Empty(Seed.Beatmapsets.Select(s => s.Id).Intersect(DeletedFromOsu));

    private static List<T> Duplicates<T>(IEnumerable<T> values) =>
        values.GroupBy(v => v).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

    [GeneratedRegex("^https://www\\.youtube\\.com/embed/[A-Za-z0-9_-]{11}$")]
    private static partial Regex EmbedLink();

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColour();
}
