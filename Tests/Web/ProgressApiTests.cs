using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Learn;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>/api/learn/progress over HTTP: who may use it and how. The merge rules are in LearnProgressControllerTests.</summary>
public class ProgressApiTests(OsbWebFactory site) : IClassFixture<OsbWebFactory>
{
    private string FirstUnit => site.Services.GetRequiredService<CourseProvider>().Current.Units[0].Id;

    private static HttpRequestMessage Post(object body, string? csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/learn/progress") { Content = JsonContent.Create(body) };
        if (csrfToken != null)
            request.Headers.Add("RequestVerificationToken", csrfToken);
        return request;
    }

    private object OneCompletion(string unit) => new { completions = new Dictionary<string, object> { [unit] = new { completedAt = "2026-10-01T10:00:00Z" } } };

    [Fact]
    public async Task WithoutLoggingIn_ItAnswers401_WithoutARedirect()
    {
        var response = await site.CreateBrowser().GetAsync("/api/learn/progress");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task ALearner_SavesProgress_AndReadsItBack()
    {
        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, 3001, "Saver");
        string token = await browser.GetCsrfTokenAsync();

        var saved = await browser.SendAsync(Post(OneCompletion(FirstUnit), token));
        var read = JsonNode.Parse(await browser.GetStringAsync("/api/learn/progress"))!;

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        Assert.Equal([FirstUnit], read["completions"]!.AsObject().Select(c => c.Key));
        Assert.Equal("2026-10-01T10:00:00Z", read["completions"]![FirstUnit]!["completedAt"]!.GetValue<string>());

        using var scope = site.Services.CreateScope();
        var learner = await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Learners.SingleAsync(l => l.Id == 3001);
        Assert.Equal("Saver", learner.Username);
    }

    [Fact]
    public async Task Saving_WithoutTheCsrfToken_IsRefused()
    {
        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, 3002, "Forgetful");

        var response = await browser.SendAsync(Post(OneCompletion(FirstUnit), csrfToken: null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EachLearner_OnlySeesTheirOwnProgress()
    {
        var first = site.CreateBrowser();
        await first.LoginAsync(site, 3003, "First");
        await first.SendAsync(Post(OneCompletion(FirstUnit), await first.GetCsrfTokenAsync()));

        var second = site.CreateBrowser();
        await second.LoginAsync(site, 3004, "Second");
        var theirs = JsonNode.Parse(await second.GetStringAsync("/api/learn/progress"))!;

        Assert.Empty(theirs["completions"]!.AsObject());
    }

    [Fact]
    public async Task LearnPages_GiveTheCsrfTokenOnlyToLoggedInLearners()
    {
        Assert.DoesNotContain("csrf-token", await site.CreateClient().GetStringAsync("/learn"));

        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, 3005, "Token Holder");
        Assert.NotEmpty(await browser.GetCsrfTokenAsync());
    }
}
