using System.Net;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>Editing and removing showcased storyboards, with the whole site in memory and the fake osu!.</summary>
public class EditPageTests : IClassFixture<OsbWebFactory>
{
    private readonly OsbWebFactory site;

    public EditPageTests(OsbWebFactory site) => this.site = site;

    /// <summary>Puts beatmapset <paramref name="id"/> in the showcase as Artist - Song {id}, by Storyboarder {id}.</summary>
    private async Task ShowcasedAsync(int id)
    {
        using var scope = site.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OsbDbContext>();
        var storyboarder = new OsuUser { Id = id + 100, Username = $"Storyboarder {id}" };
        db.Beatmapsets.Add(new Beatmapset
        {
            Id = id, Title = $"Song {id}", Artist = "Artist", Host = storyboarder, Medium = "Storybrew",
            SubmittedOn = new DateOnly(2025, 5, 1), ShowcasedOn = new DateOnly(2025, 6, 1),
            Tags = await db.Tags.Where(t => t.Slug == "particles").ToListAsync(),
            Storyboarders = [new BeatmapsetStoryboarder { User = storyboarder, Position = 0 }],
        });
        await db.SaveChangesAsync();
    }

    private async Task<HttpClient> LoggedInAsync(int id, string name)
    {
        var browser = site.CreateBrowser();
        await browser.LoginAsync(site, id, name);
        return browser;
    }

    private Task<HttpClient> ReviewerAsync() => LoggedInAsync(OsbWebFactory.ReviewerId, "Reviewer");

    private static FormUrlEncodedContent Entry(string token, int id, string title, string? storyboarders = null) =>
        new([
            new("__RequestVerificationToken", token),
            new("Title", title), new("Artist", "Artist"), new("Mapper", $"Storyboarder {id}"),
            new("Storyboarders", storyboarders ?? $"Storyboarder {id}"), new("Medium", "Storybrew"), new("Video", ""),
            new("Tags", "particles"), new("Tags", "lyrics"), new("SubmittedOn", "2025-05-01"), new("ShowcasedOn", "2025-06-01"),
        ]);

    private static FormUrlEncodedContent Removal(string token, string reason) =>
        new([new("__RequestVerificationToken", token), new("Reason", reason)]);

    [Fact]
    public async Task OnlyReviewers_CanEditOrRemove()
    {
        await ShowcasedAsync(4300001);
        var member = await LoggedInAsync(9201, "Curious");
        string token = await member.GetFormTokenAsync("/");

        var visitorEdit = await site.CreateBrowser().GetAsync("/showcase/edit?beatmapsetID=4300001");
        var memberEdit = await member.GetAsync("/showcase/edit?beatmapsetID=4300001");
        var memberRemove = await member.PostAsync("/showcase/edit/remove?beatmapsetID=4300001", Removal(token, "Mine now."));
        string memberPage = await member.GetStringAsync("/showcase/detail?beatmapsetID=4300001");
        string reviewerPage = await (await ReviewerAsync()).GetStringAsync("/showcase/detail?beatmapsetID=4300001");

        Assert.StartsWith("/auth", visitorEdit.Headers.Location!.AbsolutePath);
        Assert.Equal(HttpStatusCode.NotFound, memberEdit.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, memberRemove.StatusCode);
        Assert.DoesNotContain("/showcase/edit", memberPage);
        Assert.Contains("href=\"/showcase/edit?beatmapsetID=4300001\"", reviewerPage);
    }

    [Fact]
    public async Task TheEditPage_IsFilledInFromTheStoryboard()
    {
        await ShowcasedAsync(4300002);

        string html = await (await ReviewerAsync()).GetStringAsync("/showcase/edit?beatmapsetID=4300002");

        Assert.Contains("value=\"Song 4300002\"", html);
        Assert.Contains(">Storyboarder 4300002</textarea>", html);
        Assert.Contains("value=\"2025-06-01\"", html);
        Assert.Contains("Its current tags are ticked.", html);
        Assert.Contains("Remove from the showcase", html);
    }

    [Fact]
    public async Task Saving_ChangesTheStoryboardsPage()
    {
        await ShowcasedAsync(4300003);
        var reviewer = await ReviewerAsync();
        string token = await reviewer.GetFormTokenAsync("/showcase/edit?beatmapsetID=4300003");

        var response = await reviewer.PostAsync("/showcase/edit?beatmapsetID=4300003", Entry(token, 4300003, "Song 4300003 (TV Size)"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/showcase/detail?beatmapsetID=4300003", response.Headers.Location!.OriginalString);
        string page = await reviewer.GetStringAsync("/showcase/detail?beatmapsetID=4300003");
        Assert.Contains("Saved your changes.", page);
        Assert.Contains("Song 4300003 (TV Size)", page);
        Assert.Contains("href=\"/showcase?t=lyrics\"", page);
    }

    [Fact]
    public async Task Problems_AreShownOnTheForm()
    {
        await ShowcasedAsync(4300004);
        var reviewer = await ReviewerAsync();
        string token = await reviewer.GetFormTokenAsync("/showcase/edit?beatmapsetID=4300004");

        var unknown = await reviewer.PostAsync("/showcase/edit?beatmapsetID=4300004", Entry(token, 4300004, "Song 4300004", storyboarders: "Ghost"));
        var noTitle = await reviewer.PostAsync("/showcase/edit?beatmapsetID=4300004", Entry(token, 4300004, ""));

        Assert.Equal(HttpStatusCode.OK, unknown.StatusCode);
        Assert.Contains("osu! has no player called Ghost.", await unknown.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, noTitle.StatusCode);
        Assert.Contains("id=\"Title-error\"", await noTitle.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Removing_NeedsAReason_ThenTheStoryboardIsGone()
    {
        await ShowcasedAsync(4300005);
        var reviewer = await ReviewerAsync();
        string token = await reviewer.GetFormTokenAsync("/showcase/edit?beatmapsetID=4300005");

        var withoutReason = await reviewer.PostAsync("/showcase/edit/remove?beatmapsetID=4300005", Removal(token, " "));
        var removed = await reviewer.PostAsync("/showcase/edit/remove?beatmapsetID=4300005", Removal(token, "A duplicate."));
        string showcase = await reviewer.GetStringAsync("/showcase");
        var again = await reviewer.PostAsync("/showcase/edit/remove?beatmapsetID=4300005", Removal(token, "A duplicate."));
        string showcaseAgain = await reviewer.GetStringAsync("/showcase");

        string retry = await withoutReason.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, withoutReason.StatusCode);
        Assert.Contains("Give a reason for removing it.", retry);
        Assert.Contains("<details class=\"mt-4\" open=\"open\">", retry);
        Assert.Equal("/showcase", removed.Headers.Location!.OriginalString);
        Assert.Contains("Removed Artist - Song 4300005 from the showcase.", showcase);
        Assert.Equal(HttpStatusCode.NotFound, (await reviewer.GetAsync("/showcase/detail?beatmapsetID=4300005")).StatusCode);
        Assert.Equal("/showcase", again.Headers.Location!.OriginalString);
        Assert.Contains("Reviewer already removed this storyboard.", showcaseAgain);
    }

    [Fact]
    public async Task ARemovedStoryboard_CanBeSubmittedAgain_AndItsReviewSaysWhyItWasRemoved()
    {
        await ShowcasedAsync(4300006);
        site.Osu.Beatmapsets[4300006] = new WebBeatmapsetModel
        {
            ID = 4300006, Title = "Song 4300006", Artist = "Artist", Creator = "Storyboarder 4300006", UserID = 4300106, Storyboard = true,
            SubmittedDate = new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero),
        };
        site.Osu.Profiles[4300106] = new WebUserModel { ID = 4300106, Username = "Storyboarder 4300006" };
        var reviewer = await ReviewerAsync();
        string token = await reviewer.GetFormTokenAsync("/showcase/edit?beatmapsetID=4300006");
        await reviewer.PostAsync("/showcase/edit/remove?beatmapsetID=4300006", Removal(token, "The video was taken down."));

        var member = await LoggedInAsync(9202, "Fan");
        string memberToken = await member.GetFormTokenAsync("/showcase/submit");
        var submitted = await member.PostAsync("/showcase/submit", new FormUrlEncodedContent([
            new("__RequestVerificationToken", memberToken), new("Beatmapset", "4300006"),
            new("Storyboarders", "Storyboarder 4300006"), new("Medium", "Storybrew"),
        ]));
        int id;
        using (var scope = site.Services.CreateScope())
            id = await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Submissions
                .Where(s => s.BeatmapsetId == 4300006).Select(s => s.Id).SingleAsync();
        string review = await reviewer.GetStringAsync($"/showcase/review/{id}");

        Assert.Equal(HttpStatusCode.Redirect, submitted.StatusCode);
        Assert.Contains("Reviewer removed it from the showcase on", review);
        Assert.Contains("The video was taken down.", review);
    }
}
