using System.Net;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Learn;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>Every page of the site renders, logged out and logged in.</summary>
public class PageTests(OsbWebFactory site) : IClassFixture<OsbWebFactory>
{
    public static TheoryData<string> MainPages =>
    [
        "/", "/showcase", "/showcase?page=2", "/showcase?t=particles", "/showcase?m=Storybrew", "/showcase?s=xi",
        "/showcase/search?s=xi", "/showcase/submit", "/community", "/learn", "/learn/playground", "/learn/progress",
    ];

    [Theory]
    [MemberData(nameof(MainPages))]
    public async Task MainPages_Render(string path)
    {
        var response = await site.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<main id=\"main\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task EveryStoryboardPage_Renders()
    {
        List<int> ids;
        using (var scope = site.Services.CreateScope())
            ids = await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Beatmapsets.Select(s => s.Id).ToListAsync();

        Assert.NotEmpty(ids);
        await AssertAllRender(ids.Select(id => $"/showcase/detail?beatmapsetID={id}"));
    }

    [Fact]
    public async Task EveryProfilePage_Renders()
    {
        List<int> ids;
        using (var scope = site.Services.CreateScope())
        {
            ids = await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Users
                .Where(u => u.IsCommunityMember || u.Storyboards.Any())
                .Select(u => u.Id)
                .ToListAsync();
        }

        Assert.NotEmpty(ids);
        await AssertAllRender(ids.Select(id => $"/community/profile?userID={id}"));
    }

    [Fact]
    public async Task EveryLearnModuleAndUnit_Renders()
    {
        var course = site.Services.GetRequiredService<CourseProvider>().Current;

        await AssertAllRender(course.Modules.Select(m => m.Url).Concat(course.Units.Select(u => u.Url)));
    }

    /// <summary>Logged-in pages show the account menu, with icons and links nobody sees logged out.</summary>
    [Fact]
    public async Task PagesRender_WhenLoggedIn()
    {
        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, 1001, "Page Tester");
        var course = site.Services.GetRequiredService<CourseProvider>().Current;

        foreach (string path in new[] { "/", "/showcase", "/learn", "/learn/progress", course.Units[0].Url })
        {
            var response = await browser.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Account menu for Page Tester", await response.Content.ReadAsStringAsync());
        }
    }

    private async Task AssertAllRender(IEnumerable<string> paths)
    {
        var client = site.CreateClient();
        var failures = new List<string>();
        foreach (string path in paths)
        {
            var response = await client.GetAsync(path);
            if (response.StatusCode != HttpStatusCode.OK)
                failures.Add($"{path}: {(int)response.StatusCode}");
        }
        Assert.Empty(failures);
    }
}
