using System.Net;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>The site with output caching switched on, to check who gets a cacheable response.</summary>
public sealed class CachingSite : OsbWebFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("OutputCache:Enabled", "true");
    }
}

public class OutputCacheTests(CachingSite site) : IClassFixture<CachingSite>
{
    [Fact]
    public async Task AnonymousContentPages_AreMarkedPubliclyCacheable()
    {
        var response = await site.CreateClient().GetAsync("/showcase");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("public", response.Headers.CacheControl?.ToString() ?? "");
    }

    [Fact]
    public async Task LoggedInRequests_AreNotPubliclyCacheable()
    {
        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, 7100, "Member");

        var response = await browser.GetAsync("/showcase");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("public", response.Headers.CacheControl?.ToString() ?? "");
    }
}
