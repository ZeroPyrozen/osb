using System.Net;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>The review queue and review pages, with the whole site in memory and the fake osu!.</summary>
public class ReviewPageTests : IClassFixture<OsbWebFactory>
{
    private readonly OsbWebFactory site;

    public ReviewPageTests(OsbWebFactory site)
    {
        this.site = site;
        for (int id = 4100001; id <= 4100004; id++)
            site.Osu.Beatmapsets[id] = new WebBeatmapsetModel { ID = id, Title = $"Song {id}", Artist = "Artist", Creator = "Mapper", UserID = 5100, Storyboard = true, SubmittedDate = new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero) };
        site.Osu.Profiles[5101] = new WebUserModel { ID = 5101, Username = "Storyboarder" };
    }

    private async Task<HttpClient> LoggedInAsync(int id, string name)
    {
        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, id, name);
        return browser;
    }

    /// <summary>A member submits beatmapset <paramref name="beatmapsetId"/>; returns the submission's ID.</summary>
    private async Task<(HttpClient Member, int Id)> SubmittedAsync(int beatmapsetId, int memberId)
    {
        var member = await LoggedInAsync(memberId, $"Member {memberId}");
        string token = await member.GetFormTokenAsync("/showcase/submit");
        await member.PostAsync("/showcase/submit", new FormUrlEncodedContent([
            new("__RequestVerificationToken", token), new("Beatmapset", beatmapsetId.ToString()),
            new("Storyboarders", "Storyboarder"), new("Medium", "Storybrew"), new("Tags", "particles"),
        ]));
        using var scope = site.Services.CreateScope();
        int id = await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Submissions
            .Where(s => s.BeatmapsetId == beatmapsetId).Select(s => s.Id).SingleAsync();
        return (member, id);
    }

    private static FormUrlEncodedContent Review(string token, string decision, string? message = null, string title = "Song") =>
        new([
            new("__RequestVerificationToken", token), new("decision", decision),
            new("Title", title), new("Artist", "Artist"), new("Mapper", "Mapper"), new("Storyboarders", "Storyboarder"),
            new("Medium", "Storybrew"), new("Video", ""), new("Tags", "particles"),
            new("SubmittedOn", "2025-05-01"), new("ShowcasedOn", "2026-10-07"), new("Message", message ?? ""),
        ]);

    [Fact]
    public async Task Visitors_AreAskedToLogIn()
    {
        var response = await site.CreateBrowser().GetAsync("/showcase/review");

        Assert.StartsWith("/auth", response.Headers.Location!.AbsolutePath);
    }

    [Fact]
    public async Task MembersWhoArentReviewers_GetTheNotFoundPage()
    {
        var member = await LoggedInAsync(9101, "Curious");

        var queue = await member.GetAsync("/showcase/review");
        var page = await member.GetAsync("/showcase/review/1");

        Assert.Equal(HttpStatusCode.NotFound, queue.StatusCode);
        Assert.Contains("Gadzooks!", await queue.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, page.StatusCode);
        Assert.DoesNotContain("Review queue", await member.GetStringAsync("/"));
    }

    [Fact]
    public async Task Reviewers_SeeTheQueue_WithItsCountInTheAccountMenu()
    {
        await SubmittedAsync(4100001, 9102);
        var reviewer = await LoggedInAsync(OsbWebFactory.ReviewerId, "Reviewer");

        string home = await reviewer.GetStringAsync("/");
        string queue = await reviewer.GetStringAsync("/showcase/review");

        Assert.Contains("href=\"/showcase/review\"", home);
        Assert.Contains("data-review-count", home);
        Assert.Contains("Artist - Song 4100001", queue);
        Assert.Contains("From Member 9102", queue);
    }

    [Fact]
    public async Task TheReviewPage_IsFilledInFromTheSubmission()
    {
        var (_, id) = await SubmittedAsync(4100002, 9103);
        var reviewer = await LoggedInAsync(OsbWebFactory.ReviewerId, "Reviewer");

        string html = await reviewer.GetStringAsync($"/showcase/review/{id}");

        Assert.Contains("value=\"Song 4100002\"", html);
        Assert.Contains("value=\"2025-05-01\"", html);
        Assert.Contains("New to the site: Mapper and Storyboarder.", html);
        Assert.Contains("Approve and publish", html);
    }

    [Fact]
    public async Task Approving_PublishesTheStoryboard_AndTellsTheMember()
    {
        var (member, id) = await SubmittedAsync(4100003, 9104);
        var reviewer = await LoggedInAsync(OsbWebFactory.ReviewerId, "Reviewer");
        string token = await reviewer.GetFormTokenAsync($"/showcase/review/{id}");

        var response = await reviewer.PostAsync($"/showcase/review/{id}", Review(token, "approve", "Lovely work.", title: "Song 4100003"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Approved. Artist - Song 4100003 is in the showcase now.", await reviewer.GetStringAsync("/showcase/review"));
        string page = await site.CreateClient().GetStringAsync("/showcase/detail?beatmapsetID=4100003");
        Assert.Contains("Song 4100003", page);
        Assert.Contains("Storyboarder", page);
        Assert.Contains("data-review-news", await member.GetStringAsync("/"));
        string mine = await member.GetStringAsync("/showcase/submissions");
        Assert.Contains(">Showcased</span>", mine);
        Assert.Contains("Lovely work.", mine);
    }

    [Fact]
    public async Task Declining_NeedsAMessage_ThenTheMemberSeesIt()
    {
        var (member, id) = await SubmittedAsync(4100004, 9105);
        var reviewer = await LoggedInAsync(OsbWebFactory.ReviewerId, "Reviewer");
        string token = await reviewer.GetFormTokenAsync($"/showcase/review/{id}");

        var withoutMessage = await reviewer.PostAsync($"/showcase/review/{id}", Review(token, "decline"));
        var withMessage = await reviewer.PostAsync($"/showcase/review/{id}", Review(token, "decline", "The middle repeats the intro."));

        Assert.Equal(HttpStatusCode.OK, withoutMessage.StatusCode);
        Assert.Contains("Tell the member why, so they know what to change.", await withoutMessage.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, withMessage.StatusCode);
        string mine = await member.GetStringAsync("/showcase/submissions");
        Assert.Contains(">Declined</span>", mine);
        Assert.Contains("The middle repeats the intro.", mine);
    }
}
