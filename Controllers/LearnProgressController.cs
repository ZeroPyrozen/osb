#nullable enable

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Learn;
using osb.Services;

namespace osb.Controllers;

/// <summary>
/// Saves logged-in learners' progress. The browser keeps progress too (Scripts/learn/progress.js) and
/// sends completions here; the server merges them and answers with the learner's full set.
/// XP, levels and badges are derived from completions in the browser, so only completions are stored.
/// </summary>
[ApiController]
[Route("api/learn/progress")]
[Authorize]
[EnableRateLimiting(RateLimiting.Progress)]
public class LearnProgressController(OsbDbContext db, CourseProvider courses) : ControllerBase
{
    private const int MaxCompletionsPerRequest = 1000;

    /// <summary>How many times a sync re-reads and re-applies when it races another of this learner's.</summary>
    private const int MaxSaveAttempts = 4;

    public record CompletionDto(DateTime CompletedAt, int? Score, int? MaxScore, int? FirstScore = null);

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

        // Merging is idempotent, so retry when a sync from another tab or device creates this learner's
        // row (or the same completion) at the same moment: both are unique-constraint races, and
        // re-reading then re-applying converges. Without this the loser of the race surfaced as a 500.
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await MergeAsync(userId, course, body, ct);
            }
            catch (DbUpdateException) when (attempt < MaxSaveAttempts)
            {
                // Drop the failed inserts so the retry reads the rows the winner just committed.
                db.ChangeTracker.Clear();
            }
        }
    }

    /// <summary>Reads the learner's current completions, merges in the incoming ones, and saves.</summary>
    private async Task<ProgressDto> MergeAsync(int userId, Course course, ProgressDto body, CancellationToken ct)
    {
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
            int? firstScore = score != null && incoming.FirstScore is int f ? Math.Clamp(f, 0, max!.Value) : null;

            if (existing.TryGetValue(unit.Id, out var row))
            {
                if (completedAt < row.CompletedAt)
                    row.CompletedAt = completedAt;
                // The first attempt happens once: the first one saved stays.
                if (firstScore != null && row.FirstScore == null)
                    row.FirstScore = firstScore;
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
                    FirstScore = firstScore,
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
            c => new CompletionDto(DateTime.SpecifyKind(c.CompletedAt, DateTimeKind.Utc), c.Score, c.MaxScore, c.FirstScore)));
}
