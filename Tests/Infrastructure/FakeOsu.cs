using System.Collections.Concurrent;
using osb.Helpers;

namespace osb.Tests.Infrastructure;

/// <summary>
/// Stands in for the osu! API in the web tests. A login code is "user-&lt;id&gt;" for an account in
/// <see cref="Accounts"/>; profiles come from <see cref="Profiles"/>, and anything else is "not found".
/// </summary>
public sealed class FakeOsu : IOsuWebHelper
{
    public const string AuthorizeUrl = "https://osu.test/oauth/authorize";

    /// <summary>Accounts that can log in, by osu! user ID.</summary>
    public ConcurrentDictionary<int, WebUserModel> Accounts { get; } = new();

    /// <summary>Public profiles the site may look up, by osu! user ID.</summary>
    public ConcurrentDictionary<int, WebUserModel> Profiles { get; } = new();

    public string GetAuthorizationUrl(string state) => $"{AuthorizeUrl}?state={Uri.EscapeDataString(state)}";

    public Task<TokenModel?> GenerateAccessTokenAuthCode(string code) =>
        Task.FromResult(UserFor(code) != null ? new TokenModel { AccessToken = code, TokenType = "Bearer" } : null);

    public Task<WebUserModel?> GetOwnData(string token) => Task.FromResult(UserFor(token));

    public Task<WebUserModel?> GetUserData(int userId) => Task.FromResult(Profiles.GetValueOrDefault(userId));

    private WebUserModel? UserFor(string code) =>
        code.StartsWith("user-") && int.TryParse(code["user-".Length..], out int id) ? Accounts.GetValueOrDefault(id) : null;
}
