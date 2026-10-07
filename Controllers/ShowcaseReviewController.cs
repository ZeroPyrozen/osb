#nullable enable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using osb.Data;
using osb.Services;
using osb.ViewModels;

namespace osb.Controllers;

/// <summary>
/// The review queue, for reviewers only (see <see cref="ReviewerList"/>). Everyone else gets the
/// "not found" page, and visitors are asked to log in.
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

    private async Task<ReviewPageViewModel> PageAsync(ShowcaseSubmission submission, ReviewForm form, CancellationToken ct) =>
        new(submission, form,
            await showcase.GetTagsAsync(ct),
            await showcase.GetMediumsAsync(ct),
            await reviews.GetEarlierAttemptsAsync(submission, ct),
            await reviews.GetNewPeopleAsync(submission, ct));
}
