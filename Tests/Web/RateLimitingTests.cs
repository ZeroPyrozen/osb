using System.Net;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>The same site as the other web tests, but with a low auth rate limit so the test can trip it.</summary>
public sealed class ThrottledSite : OsbWebFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("RateLimits:AuthPerMinute", "3");
    }
}

/// <summary>The rate limiter in front of the abuse-prone endpoints (here, osu! login).</summary>
public class RateLimitingTests(ThrottledSite site) : IClassFixture<ThrottledSite>
{
    [Fact]
    public async Task TooManyLogins_FromOneClient_AreThrottled()
    {
        var browser = site.CreateBrowser();

        var codes = new List<HttpStatusCode>();
        for (int i = 0; i < 8; i++)
            codes.Add((await browser.GetAsync("/auth")).StatusCode);

        // The first few start the osu! login (a redirect); once over the limit the rest are turned away.
        Assert.Equal(3, codes.Count(c => c == HttpStatusCode.Redirect));
        Assert.Contains(HttpStatusCode.TooManyRequests, codes);
    }
}
