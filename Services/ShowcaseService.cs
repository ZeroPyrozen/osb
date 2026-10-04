#nullable enable

using Microsoft.EntityFrameworkCore;
using osb.Data;

namespace osb.Services;

public record MediumCount(string Medium, int Count);

public record HomeSummary(
    int StoryboardCount,
    int StoryboarderCount,
    IReadOnlyList<MediumCount> TopMediums,
    IReadOnlyList<Beatmapset> Recent,
    Beatmapset? FeaturedVideo);

public record ShowcasePage(IReadOnlyList<Beatmapset> Items, int Total, int Page, int PageSize)
{
    public int PageCount => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}

public record StoryboarderProfile(OsuUser User, IReadOnlyList<Beatmapset> Storyboards);

/// <summary>Read-only queries for the showcase and community pages.</summary>
public class ShowcaseService(OsbDbContext db)
{
    private IQueryable<Beatmapset> WithDetails => db.Beatmapsets.AsNoTracking()
        .Include(s => s.Host)
        .Include(s => s.Tags)
        .Include(s => s.Storyboarders).ThenInclude(c => c.User)
        .AsSplitQuery();

    public async Task<HomeSummary> GetHomeSummaryAsync(int recentCount, CancellationToken ct = default)
    {
        int storyboards = await db.Beatmapsets.CountAsync(ct);
        int storyboarders = await db.BeatmapsetStoryboarders.Select(c => c.OsuUserId).Distinct().CountAsync(ct);

        var mediums = await db.Beatmapsets
            .GroupBy(s => s.Medium)
            .Select(g => new MediumCount(g.Key, g.Count()))
            .ToListAsync(ct);

        var recent = await WithDetails
            .OrderByDescending(s => s.ShowcasedOn).ThenByDescending(s => s.Id)
            .Take(recentCount)
            .ToListAsync(ct);

        return new HomeSummary(
            storyboards,
            storyboarders,
            mediums.OrderByDescending(m => m.Count).ThenBy(m => m.Medium).Take(5).ToList(),
            recent,
            await GetRandomAsync(withVideo: true, ct));
    }

    /// <summary>
    /// Finds showcased storyboards. <paramref name="query"/> matches title, artist, host or storyboarder
    /// (case-insensitive); <paramref name="tag"/> is a tag slug and <paramref name="medium"/> a tool name.
    /// </summary>
    public async Task<ShowcasePage> SearchAsync(string? query, string? tag, string? medium, int page, int pageSize, CancellationToken ct = default)
    {
        var sets = WithDetails;

        if (!string.IsNullOrWhiteSpace(query))
        {
            string pattern = "%" + EscapeLike(query.Trim()) + "%";
            sets = sets.Where(s =>
                EF.Functions.Like(s.Title, pattern, "\\") ||
                EF.Functions.Like(s.Artist, pattern, "\\") ||
                EF.Functions.Like(s.Host.Username, pattern, "\\") ||
                s.Storyboarders.Any(c => EF.Functions.Like(c.User.Username, pattern, "\\")));
        }

        if (!string.IsNullOrWhiteSpace(tag))
            sets = sets.Where(s => s.Tags.Any(t => t.Slug == tag));

        if (!string.IsNullOrWhiteSpace(medium))
            sets = sets.Where(s => s.Medium == medium);

        int total = await sets.CountAsync(ct);
        int pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);

        var items = await sets
            .OrderByDescending(s => s.ShowcasedOn).ThenByDescending(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new ShowcasePage(items, total, page, pageSize);
    }

    public Task<Beatmapset?> GetAsync(int beatmapsetId, CancellationToken ct = default) =>
        WithDetails.FirstOrDefaultAsync(s => s.Id == beatmapsetId, ct);

    /// <summary>Other showcased storyboards by any of the same storyboarders, newest first.</summary>
    public async Task<List<Beatmapset>> GetRelatedAsync(Beatmapset set, int count, CancellationToken ct = default)
    {
        var storyboarderIds = set.Storyboarders.Select(c => c.OsuUserId).ToList();
        return await WithDetails
            .Where(s => s.Id != set.Id && s.Storyboarders.Any(c => storyboarderIds.Contains(c.OsuUserId)))
            .OrderByDescending(s => s.ShowcasedOn).ThenByDescending(s => s.Id)
            .Take(count)
            .ToListAsync(ct);
    }

    public Task<List<StoryboardTag>> GetTagsAsync(CancellationToken ct = default) =>
        db.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);

    public Task<List<string>> GetMediumsAsync(CancellationToken ct = default) =>
        db.Beatmapsets.Select(s => s.Medium).Distinct().OrderBy(m => m).ToListAsync(ct);

    public Task<Beatmapset?> GetRandomAsync(bool withVideo, CancellationToken ct = default) =>
        WithDetails
            .Where(s => !withVideo || s.VideoUrl != null)
            .OrderBy(_ => EF.Functions.Random())
            .FirstOrDefaultAsync(ct);

    /// <summary>Community members, highest role first, then by name.</summary>
    public async Task<IReadOnlyList<OsuUser>> GetCommunityAsync(CancellationToken ct = default)
    {
        var members = await db.Users.AsNoTracking()
            .Include(u => u.Roles)
            .Where(u => u.IsCommunityMember)
            .ToListAsync(ct);

        return members
            .OrderByDescending(u => u.PrimaryRole?.Rank ?? int.MinValue)
            .ThenBy(u => u.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// A storyboarder's profile: a community member or anyone credited on a showcased storyboard.
    /// Returns null for unknown users and for hosts who never storyboarded.
    /// </summary>
    public async Task<StoryboarderProfile?> GetStoryboarderAsync(int userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
            return null;

        var storyboards = await WithDetails
            .Where(s => s.Storyboarders.Any(c => c.OsuUserId == userId))
            .OrderByDescending(s => s.ShowcasedOn).ThenByDescending(s => s.Id)
            .ToListAsync(ct);

        if (!user.IsCommunityMember && storyboards.Count == 0)
            return null;

        return new StoryboarderProfile(user, storyboards);
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
