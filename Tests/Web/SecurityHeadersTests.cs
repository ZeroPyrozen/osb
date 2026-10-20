using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>The CSP and hardening headers on responses, including the playground worker's own policy.</summary>
public class SecurityHeadersTests(OsbWebFactory site) : IClassFixture<OsbWebFactory>
{
    [Fact]
    public async Task EveryPage_CarriesAStrictCsp_AndHardeningHeaders()
    {
        var response = await site.CreateClient().GetAsync("/");

        string csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("script-src 'self'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
        // The document must not be able to eval — only the playground worker may (below).
        Assert.DoesNotContain("unsafe-eval", csp);
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("DENY", Assert.Single(response.Headers.GetValues("X-Frame-Options")));
    }

    [Fact]
    public async Task ThePlaygroundWorker_GetsItsOwnEvalAllowingPolicy()
    {
        var response = await site.CreateClient().GetAsync("/js/script-worker.js");

        response.EnsureSuccessStatusCode();
        string csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("'unsafe-eval'", csp);
    }
}
