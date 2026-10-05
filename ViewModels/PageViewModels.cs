#nullable enable

using osb.Data;
using osb.Helpers;
using osb.Services;

namespace osb.ViewModels;

public record HomeViewModel(HomeSummary Summary);

public record ShowcaseIndexViewModel(
    ShowcasePage Results,
    string? Query,
    string? Tag,
    string? Medium,
    IReadOnlyList<StoryboardTag> Tags,
    IReadOnlyList<string> Mediums)
{
    public bool IsFiltered => !string.IsNullOrWhiteSpace(Query) || !string.IsNullOrWhiteSpace(Tag) || !string.IsNullOrWhiteSpace(Medium);

    /// <summary>The showcase URL with the current filters, changing only what's passed in.</summary>
    public string Url(int? page = null, string? tag = "\0", string? medium = "\0")
    {
        var query = new List<string>();
        void Add(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                query.Add($"{key}={Uri.EscapeDataString(value)}");
        }

        Add("s", Query);
        Add("t", tag == "\0" ? Tag : tag);
        Add("m", medium == "\0" ? Medium : medium);
        if (page is > 1)
            query.Add($"page={page}");
        return "/showcase" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
    }
}

public record ShowcaseDetailViewModel(Beatmapset Set, IReadOnlyList<Beatmapset> Related);

public record CommunityViewModel(IReadOnlyList<OsuUser> Members)
{
    /// <summary>Roles held by at least one member, highest rank first, for the filter chips.</summary>
    public IReadOnlyList<CommunityRole> Roles => Members
        .SelectMany(m => m.Roles)
        .DistinctBy(r => r.Id)
        .OrderByDescending(r => r.Rank)
        .ToList();
}

public record ProfileViewModel(StoryboarderProfile Profile, WebUserModel? Osu, string BannerUrl)
{
    public OsuUser User => Profile.User;

    /// <summary>The SVG flag for the user's country, when osu! told us the country and we have its flag.</summary>
    public string? FlagUrl { get; init; }
}

public record ErrorViewModel(string? RequestedPath, Beatmapset? RandomStoryboard, int StatusCode = 500);
