#nullable enable

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace osb.Data;

/// <summary>
/// Loads the initial showcase data from <c>Data/Seed/showcase.json</c> (an embedded resource).
/// Runs after migrations on every start, but only inserts rows that don't exist yet, matched by
/// osu! ID, role name or tag slug. New entries added to the JSON therefore show up on the next
/// start, and rows edited in the database are never overwritten. The one exception fills a gap:
/// a storyboard without a video gets the one the JSON has.
/// </summary>
public static class ShowcaseSeeder
{
    private const string ResourceName = "osb.Data.Seed.showcase.json";

    public static void Seed(DbContext context) =>
        SeedAsync(context, CancellationToken.None).GetAwaiter().GetResult();

    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        var db = (OsbDbContext)context;
        var seed = await LoadAsync(ct);

        var roles = await db.Roles.ToDictionaryAsync(r => r.Name, ct);
        foreach (var role in seed.Roles.Where(r => !roles.ContainsKey(r.Name)))
            db.Roles.Add(roles[role.Name] = new CommunityRole { Name = role.Name, Colour = role.Colour, Rank = role.Rank });

        var tags = await db.Tags.ToDictionaryAsync(t => t.Slug, ct);
        foreach (var tag in seed.Tags.Where(t => !tags.ContainsKey(t.Slug)))
            db.Tags.Add(tags[tag.Slug] = new StoryboardTag { Slug = tag.Slug, Name = tag.Name, Rating = tag.Rating });

        var users = await db.Users.ToDictionaryAsync(u => u.Id, ct);
        foreach (var user in seed.Users.Where(u => !users.ContainsKey(u.Id)))
        {
            db.Users.Add(users[user.Id] = new OsuUser
            {
                Id = user.Id,
                Username = user.Username,
                IsCommunityMember = user.CommunityMember,
                Roles = user.Roles.Select(name => Find(roles, name, $"role '{name}' of user {user.Id}")).ToList(),
            });
        }

        var existingSets = await db.Beatmapsets.ToDictionaryAsync(s => s.Id, ct);
        foreach (var set in seed.Beatmapsets)
        {
            if (existingSets.TryGetValue(set.Id, out var existing))
            {
                if (existing.VideoUrl == null && set.Video != null)
                    existing.VideoUrl = set.Video;
                continue;
            }

            db.Beatmapsets.Add(new Beatmapset
            {
                Id = set.Id,
                Title = set.Title,
                Artist = set.Artist,
                Host = Find(users, set.Host, $"host {set.Host} of beatmapset {set.Id}"),
                Medium = set.Medium,
                SubmittedOn = set.Submitted,
                ShowcasedOn = set.Showcased,
                VideoUrl = set.Video,
                Tags = set.Tags.Select(slug => Find(tags, slug, $"tag '{slug}' of beatmapset {set.Id}")).ToList(),
                Storyboarders = set.Storyboarders
                    .Select((id, position) => new BeatmapsetStoryboarder
                    {
                        User = Find(users, id, $"storyboarder {id} of beatmapset {set.Id}"),
                        Position = position,
                    })
                    .ToList(),
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task<SeedFile> LoadAsync(CancellationToken ct)
    {
        await using var stream = typeof(ShowcaseSeeder).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} is missing.");
        return await JsonSerializer.DeserializeAsync<SeedFile>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web), ct)
            ?? throw new InvalidDataException("showcase.json is empty.");
    }

    private static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> items, TKey key, string what) where TKey : notnull =>
        items.TryGetValue(key, out var value)
            ? value
            : throw new InvalidDataException($"showcase.json: unknown {what}. Add it to the file first.");

    private sealed record SeedFile(List<SeedRole> Roles, List<SeedTag> Tags, List<SeedUser> Users, List<SeedBeatmapset> Beatmapsets);
    private sealed record SeedRole(string Name, string Colour, int Rank);
    private sealed record SeedTag(string Slug, string Name, int Rating);
    private sealed record SeedUser(int Id, string Username, bool CommunityMember, List<string> Roles);
    private sealed record SeedBeatmapset(
        int Id, string Title, string Artist, int Host, string Medium, DateOnly Submitted, DateOnly Showcased,
        List<int> Storyboarders, List<string> Tags, string? Video);
}
