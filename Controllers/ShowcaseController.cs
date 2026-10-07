#nullable enable

using Microsoft.AspNetCore.Mvc;
using osb.Services;
using osb.ViewModels;

namespace osb.Controllers;

public class ShowcaseController(ShowcaseService showcase) : Controller
{
    private const int PageSize = 12;

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
        return View(new ShowcaseIndexViewModel(results, s, t, m, tags, mediums));
    }

    [HttpGet("/showcase/detail")]
    public async Task<IActionResult> Detail(int beatmapsetID, CancellationToken ct)
    {
        var set = await showcase.GetAsync(beatmapsetID, ct);
        if (set == null)
            return NotFound();

        var related = await showcase.GetRelatedAsync(set, 4, ct);
        return View(new ShowcaseDetailViewModel(set, related));
    }
}
