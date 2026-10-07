using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>The submit page and My submissions, with the whole site in memory and the fake osu!.</summary>
public partial class SubmissionPageTests : IClassFixture<OsbWebFactory>
{
    private readonly OsbWebFactory site;

    public SubmissionPageTests(OsbWebFactory site)
    {
        this.site = site;
        // Beatmapsets that aren't in the showcase, so they can be submitted.
        for (int id = 4000001; id <= 4000006; id++)
            site.Osu.Beatmapsets[id] = new WebBeatmapsetModel { ID = id, Title = $"Song {id}", Artist = "Artist", Creator = "Mapper", UserID = 5000, Storyboard = true };
        site.Osu.Profiles[5001] = new WebUserModel { ID = 5001, Username = "Storyboarder" };
    }

    private static FormUrlEncodedContent Fields(string token, string beatmapset, string storyboarders = "Storyboarder", params string[] tags) =>
        new([
            new("__RequestVerificationToken", token),
            new("Beatmapset", beatmapset),
            new("Storyboarders", storyboarders),
            new("Medium", "Storybrew"),
            new("Video", ""),
            new("Note", ""),
            .. tags.Select(t => new KeyValuePair<string, string>("Tags", t)),
        ]);

    private async Task<HttpClient> MemberAsync(int id, string name)
    {
        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, id, name);
        return browser;
    }

    [Fact]
    public async Task Visitors_SeeWhatWeLookFor_AndALoginButton()
    {
        string html = await site.CreateClient().GetStringAsync("/showcase/submit");

        Assert.Contains("What we look for", html);
        Assert.Contains("Log in with osu! to submit", html);
        Assert.DoesNotContain("name=\"Beatmapset\"", html);
    }

    [Fact]
    public async Task Members_GetTheForm_WithTheShowcasesTagsAndTools()
    {
        var browser = await MemberAsync(6001, "Form Viewer");

        string html = await browser.GetStringAsync("/showcase/submit");

        Assert.Contains("name=\"Beatmapset\"", html);
        Assert.Contains("value=\"particles\"", html);
        Assert.Contains("<option value=\"Storybrew\">", html);
    }

    [Fact]
    public async Task TheShowcase_LinksToTheSubmitPage() =>
        Assert.Contains("href=\"/showcase/submit\"", await site.CreateClient().GetStringAsync("/showcase"));

    [Fact]
    public async Task SubmittingOrOpeningMySubmissions_NeedsALogin()
    {
        var visitor = site.CreateBrowser();

        var submit = await visitor.PostAsync("/showcase/submit", new FormUrlEncodedContent([new("Beatmapset", "4000001")]));
        var mine = await visitor.GetAsync("/showcase/submissions");

        Assert.StartsWith("/auth", submit.Headers.Location!.AbsolutePath);
        Assert.StartsWith("/auth", mine.Headers.Location!.AbsolutePath);
    }

    [Fact]
    public async Task SubmittingWithoutTheFormToken_IsRefused()
    {
        var browser = await MemberAsync(6002, "No Token");

        var response = await browser.PostAsync("/showcase/submit", Fields("not-a-token", "4000001"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ASubmission_ShowsUpOnMySubmissions_WaitingForReview()
    {
        var browser = await MemberAsync(6003, "Submitter");
        string token = await browser.GetFormTokenAsync("/showcase/submit");

        var response = await browser.PostAsync("/showcase/submit", Fields(token, "https://osu.ppy.sh/beatmapsets/4000002", "Storyboarder", "particles"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/showcase/submissions", response.Headers.Location!.OriginalString);
        string html = await browser.GetStringAsync("/showcase/submissions");
        Assert.Contains("Thanks! The osb team will review it soon.", html);
        Assert.Contains("Artist - Song 4000002", html);
        Assert.Contains("Waiting for review", html);
        Assert.Contains("Storyboarded by Storyboarder", html);
    }

    [Fact]
    public async Task AFormWithMistakes_ComesBack_WithTheMessages_AndWhatWasTyped()
    {
        var browser = await MemberAsync(6004, "Typo");
        string token = await browser.GetFormTokenAsync("/showcase/submit");

        var response = await browser.PostAsync("/showcase/submit", Fields(token, "my favourite map", "Nobody Real"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Paste a link to the beatmapset, like https://osu.ppy.sh/beatmapsets/1011020, or its ID.", html);
        Assert.Contains("value=\"my favourite map\"", html);
        Assert.Contains("aria-invalid=\"true\"", html);
    }

    [Fact]
    public async Task AStoryboardAlreadyShowcased_LinksToItsPage()
    {
        int showcased;
        using (var scope = site.Services.CreateScope())
            showcased = await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Beatmapsets.Select(s => s.Id).FirstAsync();
        var browser = await MemberAsync(6005, "Late");
        string token = await browser.GetFormTokenAsync("/showcase/submit");

        string html = await (await browser.PostAsync("/showcase/submit", Fields(token, showcased.ToString()))).Content.ReadAsStringAsync();

        Assert.Contains("This storyboard is already in the showcase.", html);
        Assert.Contains($"href=\"/showcase/detail?beatmapsetID={showcased}\"", html);
    }

    [Fact]
    public async Task AMember_CanWithdrawTheirSubmission_AndNobodyElseCan()
    {
        var owner = await MemberAsync(6006, "Owner");
        await owner.PostAsync("/showcase/submit", Fields(await owner.GetFormTokenAsync("/showcase/submit"), "4000003"));
        int id = SubmissionIds().Matches(await owner.GetStringAsync("/showcase/submissions")).Select(m => int.Parse(m.Groups[1].Value)).Single();

        var other = await MemberAsync(6007, "Other");
        var theirs = await other.PostAsync($"/showcase/submissions/{id}/withdraw", Token(await other.GetFormTokenAsync("/showcase/submit")));
        var mine = await owner.PostAsync($"/showcase/submissions/{id}/withdraw", Token(await owner.GetFormTokenAsync("/showcase/submit")));

        Assert.Equal(HttpStatusCode.NotFound, theirs.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, mine.StatusCode);
        string html = await owner.GetStringAsync("/showcase/submissions");
        Assert.Contains("Withdrawn. You can submit it again any time.", html);
        Assert.Contains(">Withdrawn</span>", html);
    }

    /// <summary>The dot on the account menu stays until the member opens My submissions.</summary>
    [Fact]
    public async Task AReview_PutsADotOnTheAccountMenu_UntilTheMemberLooks()
    {
        var browser = await MemberAsync(6008, "Waiting");
        await browser.PostAsync("/showcase/submit", Fields(await browser.GetFormTokenAsync("/showcase/submit"), "4000004"));
        Assert.DoesNotContain("data-review-news", await browser.GetStringAsync("/"));

        using (var scope = site.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Submissions
                .Where(s => s.SubmitterId == 6008)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(s => s.Status, SubmissionStatus.Declined)
                    .SetProperty(s => s.ReviewerUsername, "Reviewer")
                    .SetProperty(s => s.ReviewNote, "Please add a video."));
        }

        Assert.Contains("data-review-news", await browser.GetStringAsync("/"));
        string mine = await browser.GetStringAsync("/showcase/submissions");
        Assert.Contains("Please add a video.", mine);
        Assert.Contains(">New</span>", mine);
        Assert.DoesNotContain("data-review-news", await browser.GetStringAsync("/"));
    }

    private static FormUrlEncodedContent Token(string token) => new([new("__RequestVerificationToken", token)]);

    [GeneratedRegex("data-submission=\"(\\d+)\"")]
    private static partial Regex SubmissionIds();
}
