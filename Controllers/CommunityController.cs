#nullable enable

using Microsoft.AspNetCore.Mvc;
using osb.Helpers;
using osb.Services;
using osb.ViewModels;

namespace osb.Controllers;

public class CommunityController(ShowcaseService showcase, IOsuWebHelper osu, IWebHostEnvironment env) : Controller
{
    [HttpGet("/community")]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new CommunityViewModel(await showcase.GetCommunityAsync(ct)));

    [HttpGet("/community/profile")]
    public async Task<IActionResult> Profile(int userID, CancellationToken ct)
    {
        var profile = await showcase.GetStoryboarderAsync(userID, ct);
        if (profile == null)
            return NotFound();

        // Live osu! data (cover, country) is a nice extra: the page still works if the API is down
        // or no client secret is configured.
        var osuUser = await osu.GetUserData(userID);
        string banner = osuUser?.Cover?.URL is { Length: > 0 } cover
            ? cover
            : $"/images/banners/bg-0{userID % 5 + 1}.webp";

        string? flag = null;
        if (osuUser?.CountryCode is { Length: 2 } code &&
            System.IO.File.Exists(Path.Combine(env.WebRootPath, "images", "flags", code.ToUpperInvariant() + ".svg")))
            flag = $"/images/flags/{code.ToUpperInvariant()}.svg";

        return View(new ProfileViewModel(profile, osuUser, banner) { FlagUrl = flag });
    }
}
