using System.Net;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

public class ErrorPageTests(OsbWebFactory site) : IClassFixture<OsbWebFactory>
{
    [Theory]
    [InlineData("/no-such-page")]
    [InlineData("/showcase/detail?beatmapsetID=1")]
    [InlineData("/community/profile?userID=1")]
    [InlineData("/learn/no-such-module")]
    [InlineData("/learn/what-is-a-storyboard/no-such-unit")]
    public async Task UnknownPages_GetTheFriendlyNotFoundPage(string path)
    {
        var response = await site.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Gadzooks!", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TheNotFoundPage_ShowsTheAddressAndARandomStoryboard()
    {
        string html = await (await site.CreateClient().GetAsync("/no/such/page")).Content.ReadAsStringAsync();

        Assert.Contains("/no/such/page</code>", html);
        Assert.Contains("Take me away!", html);
    }

    [Fact]
    public async Task ApiErrors_KeepPlainStatusCodes()
    {
        var client = site.CreateBrowser();

        var unauthorized = await client.GetAsync("/api/learn/progress");
        var missing = await client.GetAsync("/api/no-such-endpoint");

        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Null(unauthorized.Headers.Location);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.DoesNotContain("Gadzooks!", await missing.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Failures_GetTheErrorPage()
    {
        var response = await site.CreateClient().GetAsync("/test/fail");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Bollocks!", html);
        Assert.DoesNotContain("A failure for the error page test.", html);
    }

    [Fact]
    public async Task TheHealthCheck_Answers()
    {
        var response = await site.CreateClient().GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}

/// <summary>Breaks the database, so it gets a site of its own.</summary>
public class ErrorPageWithoutADatabaseTests(OsbWebFactory site) : IClassFixture<OsbWebFactory>
{
    [Fact]
    public async Task TheNotFoundPage_StillWorks_WhenTheDatabaseDoesNot()
    {
        var client = site.CreateClient();
        using (var scope = site.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OsbDbContext>();
            await db.Database.ExecuteSqlRawAsync("DROP TABLE BeatmapsetTags; DROP TABLE BeatmapsetStoryboarders; DROP TABLE Beatmapsets;");
        }

        var response = await client.GetAsync("/no-such-page");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Gadzooks!", html);
        Assert.DoesNotContain("Take me away!", html);
    }
}
