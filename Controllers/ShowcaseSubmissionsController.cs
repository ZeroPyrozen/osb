#nullable enable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osb.Services;
using osb.ViewModels;

namespace osb.Controllers;

/// <summary>Members submitting storyboards for the showcase, and following what happens to them.</summary>
public class ShowcaseSubmissionsController(SubmissionService submissions, ShowcaseService showcase) : Controller
{
    private const string FlashKey = "Flash";

    /// <summary>The submit page. Visitors see what the team looks for and a login button instead of the form.</summary>
    [HttpGet("/showcase/submit")]
    public async Task<IActionResult> Submit(CancellationToken ct) => View(await PageAsync(new SubmitForm(), ct));

    [HttpPost("/showcase/submit")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitForm form, CancellationToken ct)
    {
        int? showcasedId = null;
        if (ModelState.IsValid)
        {
            var result = await submissions.SubmitAsync(form, Member.From(User), ct);
            if (result.SubmissionId != null)
            {
                TempData[FlashKey] = "Thanks! The osb team will review it soon.";
                return RedirectToAction(nameof(Index));
            }
            foreach (var (field, message) in result.Errors)
                ModelState.AddModelError(field, message);
            showcasedId = result.ShowcasedId;
        }
        return View(await PageAsync(form, ct) with { ShowcasedId = showcasedId });
    }

    /// <summary>My submissions. Opening it marks reviews as seen, which takes the dot off the account menu.</summary>
    [HttpGet("/showcase/submissions")]
    [Authorize]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var member = Member.From(User);
        var mine = await submissions.GetMineAsync(member.Id, ct);
        var showcased = await submissions.ShowcasedAsync(mine.Select(s => s.BeatmapsetId), ct);
        await submissions.MarkOutcomesSeenAsync(member.Id, ct);
        return View(new MySubmissionsViewModel(mine, showcased, TempData[FlashKey] as string));
    }

    [HttpPost("/showcase/submissions/{id:int}/withdraw")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id, CancellationToken ct)
    {
        var outcome = await submissions.WithdrawAsync(id, Member.From(User).Id, ct);
        if (outcome == WithdrawOutcome.NotFound)
            return NotFound();
        TempData[FlashKey] = outcome == WithdrawOutcome.Withdrawn
            ? "Withdrawn. You can submit it again any time."
            : "It was reviewed in the meantime, so it can't be withdrawn.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SubmitPageViewModel> PageAsync(SubmitForm form, CancellationToken ct) =>
        new(form, await showcase.GetTagsAsync(ct), await showcase.GetMediumsAsync(ct));
}
