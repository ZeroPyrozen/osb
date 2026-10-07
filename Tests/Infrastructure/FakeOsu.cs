using System.Collections.Concurrent;
using osb.Helpers;

namespace osb.Tests.Infrastructure;

/// <summary>
/// Stands in for the osu! API in the web tests. A login code is "user-&lt;id&gt;" for an account in
/// <see cref="Accounts"/>. Profiles, beatmapsets and difficulties come from the dictionaries below;
/// anything else is "not found".
/// </summary>
public sealed class FakeOsu : IOsuWebHelper
{
    public const string AuthorizeUrl = "https://osu.test/oauth/authorize";

    /// <summary>Accounts that can log in, by osu! user ID.</summary>
    public ConcurrentDictionary<int, WebUserModel> Accounts { get; } = new();

    /// <summary>Public profiles the site may look up, by osu! user ID.</summary>
    public ConcurrentDictionary<int, WebUserModel> Profiles { get; } = new();

    /// <summary>Beatmapsets osu! knows, by ID.</summary>
    public ConcurrentDictionary<int, WebBeatmapsetModel> Beatmapsets { get; } = new();

    /// <summary>Difficulties osu! knows, by beatmap ID.</summary>
    public ConcurrentDictionary<int, WebBeatmapModel> Beatmaps { get; } = new();

    /// <summary>While set, lookups fail as if osu! couldn't be reached. Logging in still works.</summary>
    public bool Unreachable { get; set; }

    public string GetAuthorizationUrl(string state) => $"{AuthorizeUrl}?state={Uri.EscapeDataString(state)}";

    public Task<TokenModel?> GenerateAccessTokenAuthCode(string code) =>
        Task.FromResult(UserFor(code) != null ? new TokenModel { AccessToken = code, TokenType = "Bearer" } : null);

    public Task<WebUserModel?> GetOwnData(string token) => Task.FromResult(UserFor(token));

    public Task<WebUserModel?> GetUserData(int userId) => Task.FromResult(Profiles.GetValueOrDefault(userId));

    public Task<OsuLookup<WebBeatmapsetModel>> GetBeatmapsetData(int beatmapsetId) => Lookup(Beatmapsets.GetValueOrDefault(beatmapsetId));

    public Task<OsuLookup<WebBeatmapModel>> GetBeatmapData(int beatmapId) => Lookup(Beatmaps.GetValueOrDefault(beatmapId));

    public Task<OsuLookup<WebUserModel>> FindUser(string idOrUsername)
    {
        string name = idOrUsername.Trim();
        var people = Profiles.Values.Concat(Accounts.Values).ToList();
        var found = int.TryParse(name, out int id) && people.FirstOrDefault(u => u.ID == id) is { } byId
            ? byId
            : people.FirstOrDefault(u => string.Equals(u.Username, name, StringComparison.OrdinalIgnoreCase));
        return Lookup(found);
    }

    private Task<OsuLookup<T>> Lookup<T>(T? value) where T : class =>
        Task.FromResult(Unreachable ? OsuLookup<T>.Failed : new OsuLookup<T>(value));

    private WebUserModel? UserFor(string code) =>
        code.StartsWith("user-") && int.TryParse(code["user-".Length..], out int id) ? Accounts.GetValueOrDefault(id) : null;
}
