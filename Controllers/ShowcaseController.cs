#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using osb.Services;
using osb.ViewModels;

namespace osb.Controllers;

[OutputCache(PolicyName = "anon")]
public class ShowcaseController(ShowcaseService showcase) : Controller
{
    private const int PageSize = 12;

    /// <summary>Messages from reviewers' edits and removals, which come back to these pages.</summary>
    private const string FlashKey = "Flash";

    /// <summary>
    /// The flash message, read only when someone is logged in. Flash only ever comes from reviewer
    /// actions, so anonymous visitors never have one — and merely reading TempData would set a cookie
    /// that stops the anonymous showcase pages being cached.
    /// </summary>
    private string? Flash() => User.Identity?.IsAuthenticated == true ? TempData[FlashKey] as string : null;

    /// <summary>
    /// The showcase list. <c>s</c> searches titles, artists, hosts and storyboarders; <c>t</c> is a tag slug,
    /// <c>m</c> a tool. <c>/showcase/search</c> is kept as an alias because old tag links point there.
    /// </summary>
    [HttpGet("/showcase")]
    [HttpGet("/showcase/search")]
    public async Task<IActionResult> Index(string? s, string? t, string? m, int page = 1, CancellationToken ct = default)
    {
        var results = await showcase.SearchAsync(s, t, m, page, PageSize, ct);
        var tags = await showcase.GetTagsAsync(ct);
        var mediums = await showcase.GetMediumsAsync(ct);
        return View(new ShowcaseIndexViewModel(results, s, t, m, tags, mediums) { Flash = Flash() });
    }

    [HttpGet("/showcase/detail")]
    public async Task<IActionResult> Detail(int beatmapsetID, CancellationToken ct)
    {
        var set = await showcase.GetAsync(beatmapsetID, ct);
        if (set == null)
            return NotFound();

        var related = await showcase.GetRelatedAsync(set, 4, ct);
        return View(new ShowcaseDetailViewModel(set, related) { Flash = Flash() });
    }
}
