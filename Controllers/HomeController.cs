#nullable enable

using Microsoft.AspNetCore.Mvc;
using osb.Services;
using osb.ViewModels;

namespace osb.Controllers;

public class HomeController(ShowcaseService showcase) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var summary = await showcase.GetHomeSummaryAsync(recentCount: 6, ct);
        return View(new HomeViewModel(summary));
    }
}
