using System.Net;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>Logging in with osu! (OAuth authorization code flow), against the fake osu!.</summary>
public class AuthTests(OsbWebFactory site) : IClassFixture<OsbWebFactory>
{
    [Fact]
    public async Task LoggingIn_StartsAtOsu_WithAShortLivedStateCookie()
    {
        var response = await site.CreateBrowser().GetAsync("/auth?returnUrl=/learn");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith(FakeOsu.AuthorizeUrl + "?state=", response.Headers.Location!.OriginalString);
        Assert.Matches("^[0-9A-F]{32}$", Browser.StateFrom(response));
        string cookie = response.SetCookies()["osb.oauth"].ToLowerInvariant();
        Assert.Contains("httponly", cookie);
        Assert.Contains("samesite=lax", cookie);
        Assert.Contains("max-age=600", cookie);
        Assert.DoesNotContain("secure", cookie);
    }

    /// <summary>On the Pi behind cloudflared or nginx, the site sees plain HTTP plus X-Forwarded-Proto.</summary>
    [Fact]
    public async Task BehindAnHttpsProxy_TheStateCookieIsSecure()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/auth");
        request.Headers.Add("X-Forwarded-Proto", "https");

        var response = await site.CreateBrowser().SendAsync(request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("secure", response.SetCookies()["osb.oauth"].ToLowerInvariant());
    }

    [Fact]
    public async Task WithTheRightState_YouAreLoggedIn_AndSentBackToThePage()
    {
        var browser = site.CreateBrowser();

        string returnedTo = await browser.LoginAsync(site, 2001, "Tester", returnUrl: "/learn/progress");

        Assert.Equal("/learn/progress", returnedTo);
        Assert.Contains("Account menu for Tester", await browser.GetStringAsync("/"));
    }

    public static TheoryData<string> BadCallbacks =>
    [
        "code=user-2002&state=WRONG",
        "code=user-2002",
        "state={state}",
        "error=access_denied&state={state}",
        "code=not-a-real-code&state={state}",
    ];

    [Theory]
    [MemberData(nameof(BadCallbacks))]
    public async Task AWrongStateACancelOrARejectedCode_DoesNotLogIn(string query)
    {
        site.Osu.Accounts[2002] = new() { ID = 2002, Username = "Nobody" };
        var browser = site.CreateBrowser();
        string state = Browser.StateFrom(await browser.GetAsync("/auth"));

        var response = await browser.GetAsync("/auth/authorized?" + query.Replace("{state}", state));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location!.OriginalString);
        Assert.DoesNotContain("osb.auth", response.SetCookies().Keys);
        Assert.Contains(">Login</a>", await browser.GetStringAsync("/"));
    }

    [Fact]
    public async Task TheCallback_WithoutTheStateCookie_DoesNotLogIn()
    {
        site.Osu.Accounts[2003] = new() { ID = 2003, Username = "Elsewhere" };
        string state = Browser.StateFrom(await site.CreateBrowser().GetAsync("/auth"));

        // A different browser, so the state cookie isn't there.
        var response = await site.CreateBrowser().GetAsync($"/auth/authorized?code=user-2003&state={state}");

        Assert.Equal("/", response.Headers.Location!.OriginalString);
        Assert.DoesNotContain("osb.auth", response.SetCookies().Keys);
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("//evil.example/")]
    [InlineData("/\\evil.example/")]
    public async Task ReturnAddressesOnOtherSites_GoToTheHomePage(string returnUrl)
    {
        string returnedTo = await site.CreateBrowser().LoginAsync(site, 2004, "Traveller", returnUrl);

        Assert.Equal("/", returnedTo);
    }

    [Fact]
    public async Task LoggingOut_NeedsAPostWithTheFormToken()
    {
        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, 2005, "Leaver");

        var withoutToken = await browser.PostAsync("/auth/logout", new FormUrlEncodedContent([]));
        Assert.Equal(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.Contains("Account menu for Leaver", await browser.GetStringAsync("/"));

        string token = await browser.GetFormTokenAsync("/");
        var withToken = await browser.PostAsync("/auth/logout", new FormUrlEncodedContent([new("__RequestVerificationToken", token)]));

        Assert.Equal(HttpStatusCode.Redirect, withToken.StatusCode);
        Assert.Contains(">Login</a>", await browser.GetStringAsync("/"));
    }
}
