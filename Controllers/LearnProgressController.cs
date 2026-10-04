#nullable enable

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Learn;

namespace osb.Controllers;

/// <summary>
/// Saves logged-in learners' progress. The browser keeps progress too (Scripts/learn/progress.js) and
/// sends completions here; the server merges them and answers with the learner's full set.
/// XP, levels and badges are derived from completions in the browser, so only completions are stored.
/// </summary>
[ApiController]
[Route("api/learn/progress")]
[Authorize]
public class LearnProgressController(OsbDbContext db, CourseProvider courses) : ControllerBase
{
    private const int MaxCompletionsPerRequest = 1000;

    public record CompletionDto(DateTime CompletedAt, int? Score, int? MaxScore);

    public record ProgressDto(Dictionary<string, CompletionDto> Completions);

    [HttpGet]
    public async Task<ProgressDto> Get(CancellationToken ct) =>
        await LoadAsync(CurrentUserId, ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ProgressDto>> Post(ProgressDto body, CancellationToken ct)
    {
        if (body.Completions.Count > MaxCompletionsPerRequest)
            return BadRequest($"Send at most {MaxCompletionsPerRequest} completions at a time.");

        int userId = CurrentUserId;
        var course = courses.Current;
        var now = DateTime.UtcNow;

        var learner = await db.Learners.FindAsync([userId], ct);
        if (learner == null)
        {
            learner = new Learner { Id = userId, JoinedAt = now };
            db.Learners.Add(learner);
        }
        learner.Username = User.Identity?.Name ?? learner.Username;
        learner.LastActiveAt = now;

        var existing = await db.UnitCompletions.Where(c => c.LearnerId == userId).ToDictionaryAsync(c => c.UnitId, ct);
        foreach (var (unitId, incoming) in body.Completions)
        {
            var unit = course.FindUnit(unitId);
            if (unit == null)
                continue; // a unit that doesn't exist (any more)

            // Clamp timestamps from the browser to something sensible.
            var completedAt = incoming.CompletedAt.ToUniversalTime();
            if (completedAt > now || completedAt.Year < 2020)
                completedAt = now;

            // Scores only mean something for knowledge checks, and can't exceed the question count.
            int? max = unit.Quiz?.Questions.Count;
            int? score = max != null && incoming.Score is int s ? Math.Clamp(s, 0, max.Value) : null;

            if (existing.TryGetValue(unit.Id, out var row))
            {
                if (completedAt < row.CompletedAt)
                    row.CompletedAt = completedAt;
                if (score != null && (row.Score == null || score > row.Score))
                {
                    row.Score = score;
                    row.MaxScore = max;
                }
            }
            else
            {
                var completion = new UnitCompletion
                {
                    LearnerId = userId,
                    UnitId = unit.Id,
                    CompletedAt = completedAt,
                    Score = score,
                    MaxScore = score != null ? max : null,
                };
                db.UnitCompletions.Add(completion);
                existing[unit.Id] = completion;
            }
        }

        await db.SaveChangesAsync(ct);
        return ToDto(existing.Values);
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<ProgressDto> LoadAsync(int userId, CancellationToken ct) =>
        ToDto(await db.UnitCompletions.AsNoTracking().Where(c => c.LearnerId == userId).ToListAsync(ct));

    private static ProgressDto ToDto(IEnumerable<UnitCompletion> rows) =>
        new(rows.ToDictionary(
            c => c.UnitId,
            c => new CompletionDto(DateTime.SpecifyKind(c.CompletedAt, DateTimeKind.Utc), c.Score, c.MaxScore)));
}
