#nullable enable

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace osb.Data;

/// <summary>
/// Loads the initial showcase data from <c>Data/Seed/showcase.json</c> (an embedded resource).
/// Runs after migrations on every start, but only adds what the database doesn't have yet, matched by
/// osu! ID, role name or tag slug. New entries added to the JSON therefore show up on the next start,
/// and rows edited in the database are never overwritten. It only fills these gaps:
/// <list type="bullet">
///   <item>A storyboard without a video gets the one the JSON has, unless it was changed on the site.</item>
///   <item>People the JSON lists as community members get that membership and their roles, even when
///   the site added them first (from a submission). Nobody loses a membership or role.</item>
/// </list>
/// Storyboards a reviewer removed (<see cref="ShowcaseRemoval"/>) are never added back.
/// </summary>
public static class ShowcaseSeeder
{
    private const string ResourceName = "osb.Data.Seed.showcase.json";

    public static void Seed(DbContext context) =>
        SeedAsync(context, CancellationToken.None).GetAwaiter().GetResult();

    public static async Task SeedAsync(DbContext context, CancellationToken ct) =>
        await ApplyAsync((OsbDbContext)context, await LoadAsync(ct), ct);

    /// <summary>Seeds from showcase.json content passed in, so tests can try files with mistakes.</summary>
    internal static async Task SeedAsync(OsbDbContext db, Stream json, CancellationToken ct) =>
        await ApplyAsync(db, await ReadAsync(json, ct), ct);

    private static async Task ApplyAsync(OsbDbContext db, SeedFile seed, CancellationToken ct)
    {
        var roles = await db.Roles.ToDictionaryAsync(r => r.Name, ct);
        foreach (var role in seed.Roles.Where(r => !roles.ContainsKey(r.Name)))
            db.Roles.Add(roles[role.Name] = new CommunityRole { Name = role.Name, Colour = role.Colour, Rank = role.Rank });

        var tags = await db.Tags.ToDictionaryAsync(t => t.Slug, ct);
        foreach (var tag in seed.Tags.Where(t => !tags.ContainsKey(t.Slug)))
            db.Tags.Add(tags[tag.Slug] = new StoryboardTag { Slug = tag.Slug, Name = tag.Name, Rating = tag.Rating });

        var users = await db.Users.Include(u => u.Roles).ToDictionaryAsync(u => u.Id, ct);
        foreach (var user in seed.Users)
        {
            var userRoles = user.Roles.Select(name => Find(roles, name, $"role '{name}' of user {user.Id}")).ToList();
            if (!users.TryGetValue(user.Id, out var known))
            {
                db.Users.Add(users[user.Id] = new OsuUser
                {
                    Id = user.Id,
                    Username = user.Username,
                    IsCommunityMember = user.CommunityMember,
                    Roles = userRoles,
                });
                continue;
            }

            if (user.CommunityMember)
                known.IsCommunityMember = true;
            foreach (var role in userRoles.Where(r => !known.Roles.Contains(r)))
                known.Roles.Add(role);
        }

        var removed = (await db.ShowcaseRemovals.Select(r => r.BeatmapsetId).ToListAsync(ct)).ToHashSet();
        var existingSets = await db.Beatmapsets.ToDictionaryAsync(s => s.Id, ct);
        foreach (var set in seed.Beatmapsets.Where(s => !removed.Contains(s.Id)))
        {
            if (existingSets.TryGetValue(set.Id, out var existing))
            {
                if (existing.ChangedOnSiteAt == null && existing.VideoUrl == null && set.Video != null)
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

    /// <summary>The embedded showcase.json, as the seeder reads it.</summary>
    internal static async Task<SeedFile> LoadAsync(CancellationToken ct)
    {
        await using var stream = typeof(ShowcaseSeeder).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} is missing.");
        return await ReadAsync(stream, ct);
    }

    private static async Task<SeedFile> ReadAsync(Stream json, CancellationToken ct) =>
        await JsonSerializer.DeserializeAsync<SeedFile>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web), ct)
            ?? throw new InvalidDataException("showcase.json is empty.");

    private static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> items, TKey key, string what) where TKey : notnull =>
        items.TryGetValue(key, out var value)
            ? value
            : throw new InvalidDataException($"showcase.json: unknown {what}. Add it to the file first.");

    internal sealed record SeedFile(List<SeedRole> Roles, List<SeedTag> Tags, List<SeedUser> Users, List<SeedBeatmapset> Beatmapsets);
    internal sealed record SeedRole(string Name, string Colour, int Rank);
    internal sealed record SeedTag(string Slug, string Name, int Rating);
    internal sealed record SeedUser(int Id, string Username, bool CommunityMember, List<string> Roles);
    internal sealed record SeedBeatmapset(
        int Id, string Title, string Artist, int Host, string Medium, DateOnly Submitted, DateOnly Showcased,
        List<int> Storyboarders, List<string> Tags, string? Video);
}
