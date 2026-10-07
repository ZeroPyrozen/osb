using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using osb.Helpers;
using osb.Tests.Infrastructure;

namespace osb.Tests.Helpers;

/// <summary>The osu! API client, against a stub osu! that answers from the test.</summary>
public class OsuWebHelperTests
{
    private const string TokenUrl = "https://osu.ppy.sh/oauth/token";
    private const string SiteToken = """{"token_type":"Bearer","expires_in":86400,"access_token":"site-token"}""";

    /// <summary>The real appsettings.json, so the shipped URL templates are what's tested.</summary>
    private static IConfiguration Settings() => new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(RepoPaths.Root, "appsettings.json"))
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["API:ClientID"] = "12345",
            ["API:ClientSecret"] = "the-secret",
            ["API:RedirectURL"] = "https://osb.example/auth/authorized",
        })
        .Build();

    private static (OsuWebHelper Osu, StubHttpHandler Http) Create(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var http = new StubHttpHandler(respond);
        var osu = new OsuWebHelper(new HttpClient(http), Settings(), new MemoryCache(new MemoryCacheOptions()), NullLogger<OsuWebHelper>.Instance);
        return (osu, http);
    }

    private static string User(int id, string name) =>
        $$$"""{"id":{{{id}}},"username":"{{{name}}}","avatar_url":"https://a.ppy.sh/{{{id}}}","country_code":"AU","country":{"code":"AU","name":"Australia"},"cover":{"url":"https://assets.ppy.sh/cover-{{{id}}}.jpg"}}""";

    [Fact]
    public void TheLoginLink_CarriesTheClientCallbackScopeAndState()
    {
        var (osu, _) = Create(_ => throw new InvalidOperationException("No request expected."));

        Assert.Equal(
            "https://osu.ppy.sh/oauth/authorize?client_id=12345&redirect_uri=https%3A%2F%2Fosb.example%2Fauth%2Fauthorized&response_type=code&scope=identify&state=a%20b%26c",
            osu.GetAuthorizationUrl("a b&c"));
    }

    [Fact]
    public async Task ExchangingACode_PostsTheGrant_AndReturnsTheToken()
    {
        var (osu, http) = Create(_ => StubHttpHandler.Json("""{"token_type":"Bearer","expires_in":86400,"access_token":"user-token","refresh_token":"r"}"""));

        var token = await osu.GenerateAccessTokenAuthCode("the-code");

        Assert.Equal("user-token", token?.AccessToken);
        var request = Assert.Single(http.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(TokenUrl, request.Url);
        var grant = JsonNode.Parse(request.Body!)!;
        Assert.Equal(12345, grant["client_id"]!.GetValue<int>());
        Assert.Equal("the-secret", grant["client_secret"]!.GetValue<string>());
        Assert.Equal("the-code", grant["code"]!.GetValue<string>());
        Assert.Equal("authorization_code", grant["grant_type"]!.GetValue<string>());
        Assert.Equal("https://osb.example/auth/authorized", grant["redirect_uri"]!.GetValue<string>());
    }

    public static TheoryData<string> Failures => ["rejected", "server error", "not json", "network down", "timeout"];

    private static HttpResponseMessage Fail(string how) => how switch
    {
        "rejected" => StubHttpHandler.Json("""{"error":"invalid_grant"}""", HttpStatusCode.BadRequest),
        "server error" => new HttpResponseMessage(HttpStatusCode.InternalServerError),
        "not json" => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<html>Down for maintenance</html>") },
        "network down" => throw new HttpRequestException("No route to host"),
        _ => throw new TaskCanceledException("Timed out"),
    };

    [Theory]
    [MemberData(nameof(Failures))]
    public async Task ExchangingACode_FailsQuietly(string how)
    {
        var (osu, _) = Create(_ => Fail(how));

        Assert.Null(await osu.GenerateAccessTokenAuthCode("the-code"));
    }

    [Fact]
    public async Task OwnData_IsReadWithTheLearnersToken()
    {
        var (osu, http) = Create(_ => StubHttpHandler.Json(User(2, "peppy")));

        var user = (await osu.GetOwnData("user-token"))!;

        Assert.Equal((2, "peppy", "AU", "Australia"), (user.ID, user.Username, user.CountryCode, user.Country.Name));
        Assert.Equal("https://assets.ppy.sh/cover-2.jpg", user.Cover.URL);
        var request = Assert.Single(http.Requests);
        Assert.Equal("https://osu.ppy.sh/api/v2/me", request.Url);
        Assert.Equal("Bearer user-token", request.Authorization);
    }

    [Fact]
    public async Task Profiles_ShareOneSiteToken_AndAreCached()
    {
        var (osu, http) = Create(request => request.RequestUri!.ToString() == TokenUrl
            ? StubHttpHandler.Json(SiteToken)
            : StubHttpHandler.Json(User(int.Parse(request.RequestUri.Segments[^1]), "someone")));

        Assert.Equal(2, (await osu.GetUserData(2))?.ID);
        Assert.Equal(3, (await osu.GetUserData(3))?.ID);
        Assert.Equal(2, (await osu.GetUserData(2))?.ID);

        Assert.Equal([TokenUrl, "https://osu.ppy.sh/api/v2/users/2", "https://osu.ppy.sh/api/v2/users/3"], http.Requests.Select(r => r.Url));
        var grant = JsonNode.Parse(http.Requests[0].Body!)!;
        Assert.Equal("client_credentials", grant["grant_type"]!.GetValue<string>());
        Assert.Equal("public", grant["scope"]!.GetValue<string>());
        Assert.All(http.Requests.Skip(1), r => Assert.Equal("Bearer site-token", r.Authorization));
    }

    /// <summary>With a wrong or missing client secret, every profile view would otherwise ask osu! for a token.</summary>
    [Fact]
    public async Task AFailedSiteToken_IsNotAskedForAgainStraightAway()
    {
        var (osu, http) = Create(_ => StubHttpHandler.Json("""{"error":"invalid_client"}""", HttpStatusCode.Unauthorized));

        Assert.Null(await osu.GetUserData(2));
        Assert.Null(await osu.GetUserData(3));

        Assert.Equal([TokenUrl], http.Requests.Select(r => r.Url));
    }

    [Theory]
    [MemberData(nameof(Failures))]
    public async Task ProfileLookups_FailQuietly(string how)
    {
        var (osu, _) = Create(request => request.RequestUri!.ToString() == TokenUrl ? StubHttpHandler.Json(SiteToken) : Fail(how));

        Assert.Null(await osu.GetUserData(2));
    }
}
