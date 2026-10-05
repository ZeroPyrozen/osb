#nullable enable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using osb.Data;
using osb.Services;
using osb.ViewModels;

namespace osb.Controllers;

[AllowAnonymous]
public class ErrorController(ShowcaseService showcase, ILogger<ErrorController> logger) : Controller
{
    [Route("Error/{statusCode:int}")]
    public async Task<IActionResult> HttpStatusCodeHandler(int statusCode, CancellationToken ct)
    {
        var reExecute = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        Response.StatusCode = statusCode;
        return View(statusCode == 404 ? "NotFound" : "InternalError",
            new ErrorViewModel(reExecute?.OriginalPath, await TryGetRandomAsync(ct), statusCode));
    }

    [Route("Error")]
    public async Task<IActionResult> Error(CancellationToken ct)
    {
        var failure = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        return View("InternalError", new ErrorViewModel(failure?.Path, await TryGetRandomAsync(ct)));
    }

    /// <summary>The "take me away" storyboard. Never lets a broken database break the error page too.</summary>
    private async Task<Beatmapset?> TryGetRandomAsync(CancellationToken ct)
    {
        try
        {
            return await showcase.GetRandomAsync(withVideo: false, ct);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Couldn't pick a random storyboard for the error page.");
            return null;
        }
    }
}
