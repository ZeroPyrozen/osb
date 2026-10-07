#nullable enable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osb.Data;
using osb.Services;
using osb.ViewModels;

namespace osb.Controllers;

/// <summary>
/// Reviewers' pages: the review queue, and editing or removing showcased storyboards. Only reviewers
/// (see <see cref="ReviewerList"/>) get them; everyone else gets the "not found" page, and visitors
/// are asked to log in.
/// </summary>
[Authorize(Roles = ReviewerList.Role)]
public class ShowcaseReviewController(ReviewService reviews, ShowcaseService showcase) : Controller
{
    private const string FlashKey = "Flash";

    [HttpGet("/showcase/review")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var (waiting, recent) = await reviews.GetQueueAsync(ct);
        return View(new ReviewQueueViewModel(waiting, recent, TempData[FlashKey] as string));
    }

    [HttpGet("/showcase/review/{id:int}")]
    public async Task<IActionResult> Review(int id, CancellationToken ct)
    {
        var submission = await reviews.GetAsync(id, ct);
        if (submission == null)
            return NotFound();
        return View(await PageAsync(submission, ReviewService.FormFor(submission), ct));
    }

    /// <param name="decision">Which button was pressed: "approve" or "decline".</param>
    [HttpPost("/showcase/review/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, ReviewForm form, string? decision, CancellationToken ct)
    {
        var submission = await reviews.GetAsync(id, ct);
        if (submission == null)
            return NotFound();

        ReviewResult result;
        if (decision == "decline")
        {
            // Only the message matters when declining; the entry fields may be half edited.
            ModelState.Clear();
            result = await reviews.DeclineAsync(id, form.Message, Member.From(User), ct);
        }
        else if (ModelState.IsValid)
        {
            result = await reviews.ApproveAsync(id, form, Member.From(User), ct);
        }
        else
        {
            return View(await PageAsync(submission, form, ct));
        }

        switch (result.Outcome)
        {
            case ReviewOutcome.NotFound:
                return NotFound();
            case ReviewOutcome.Approved:
                TempData[FlashKey] = $"Approved. {submission.Artist} - {submission.Title} is in the showcase now.";
                return RedirectToAction(nameof(Index));
            case ReviewOutcome.Declined:
                TempData[FlashKey] = $"Declined. {submission.SubmitterUsername} will see your message.";
                return RedirectToAction(nameof(Index));
            case ReviewOutcome.AlreadyHandled:
                TempData[FlashKey] = result.Message;
                return RedirectToAction(nameof(Index));
            case ReviewOutcome.AlreadyShowcased:
                ModelState.AddModelError("", "This storyboard is already in the showcase, so it can't be approved. Decline it as a duplicate.");
                break;
            default:
                foreach (var (field, message) in result.Errors)
                    ModelState.AddModelError(field, message);
                break;
        }
        return View(await PageAsync(submission, form, ct));
    }

    [HttpGet("/showcase/edit")]
    public async Task<IActionResult> Edit(int beatmapsetID, CancellationToken ct)
    {
        var set = await showcase.GetAsync(beatmapsetID, ct);
        if (set == null)
            return NotFound();
        return View(await EditPageAsync(set, ReviewService.FormFor(set), ct));
    }

    [HttpPost("/showcase/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int beatmapsetID, EntryForm form, CancellationToken ct)
    {
        var set = await showcase.GetAsync(beatmapsetID, ct);
        if (set == null)
            return await GoneAsync(beatmapsetID, ct);

        if (ModelState.IsValid)
        {
            var result = await reviews.EditAsync(beatmapsetID, form, ct);
            if (result.Outcome == ReviewOutcome.Saved)
            {
                TempData[FlashKey] = "Saved your changes.";
                return Redirect($"/showcase/detail?beatmapsetID={beatmapsetID}");
            }
            if (result.Outcome == ReviewOutcome.NotFound)
                return await GoneAsync(beatmapsetID, ct);
            foreach (var (field, message) in result.Errors)
                ModelState.AddModelError(field, message);
        }
        return View(await EditPageAsync(set, form, ct));
    }

    [HttpPost("/showcase/edit/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int beatmapsetID, string? reason, CancellationToken ct)
    {
        var set = await showcase.GetAsync(beatmapsetID, ct);
        if (set == null)
            return await GoneAsync(beatmapsetID, ct);

        var result = await reviews.RemoveAsync(beatmapsetID, reason, Member.From(User), ct);
        if (result.Outcome == ReviewOutcome.Removed)
        {
            TempData[FlashKey] = $"Removed {set.Artist} - {set.Title} from the showcase.";
            return Redirect("/showcase");
        }
        if (result.Outcome == ReviewOutcome.NotFound)
            return await GoneAsync(beatmapsetID, ct);
        foreach (var (field, message) in result.Errors)
            ModelState.AddModelError(field, message);
        return View(nameof(Edit), await EditPageAsync(set, ReviewService.FormFor(set), ct, reason));
    }

    /// <summary>
    /// After a form was sent for a storyboard that's no longer in the showcase: says who removed it (perhaps
    /// a moment ago, from another tab, or with a second click), or "not found" if nobody did.
    /// </summary>
    private async Task<IActionResult> GoneAsync(int beatmapsetId, CancellationToken ct)
    {
        if (await reviews.GetRemovalAsync(beatmapsetId, ct) is not { } removal)
            return NotFound();
        TempData[FlashKey] = $"{removal.RemovedByUsername} already removed this storyboard.";
        return Redirect("/showcase");
    }

    private async Task<ReviewPageViewModel> PageAsync(ShowcaseSubmission submission, ReviewForm form, CancellationToken ct) =>
        new(submission, form,
            await showcase.GetTagsAsync(ct),
            await showcase.GetMediumsAsync(ct),
            await reviews.GetEarlierAttemptsAsync(submission, ct),
            await reviews.GetNewPeopleAsync(submission, ct),
            await reviews.GetRemovalAsync(submission.BeatmapsetId, ct));

    private async Task<EditPageViewModel> EditPageAsync(Beatmapset set, EntryForm form, CancellationToken ct, string? reason = null) =>
        new(set, form, await showcase.GetTagsAsync(ct), await showcase.GetMediumsAsync(ct), reason);
}
