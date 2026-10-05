#nullable enable

namespace osb.Data;

/// <summary>A logged-in learner. The key is the osu! user ID.</summary>
public class Learner
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public DateTime JoinedAt { get; set; }
    public DateTime LastActiveAt { get; set; }

    public List<UnitCompletion> Completions { get; set; } = new();
}

/// <summary>
/// A learn unit a learner has finished. XP, levels, badges and streaks are all derived from these rows
/// and the course catalog, so changing the XP rules never requires a data migration.
/// </summary>
public class UnitCompletion
{
    public int Id { get; set; }

    public int LearnerId { get; set; }
    public Learner Learner { get; set; } = null!;

    /// <summary>The unit's ID in the catalog, <c>module-slug/unit-slug</c>.</summary>
    public string UnitId { get; set; } = "";

    /// <summary>When the unit was first completed (UTC).</summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>For knowledge checks: the best number of correct answers so far.</summary>
    public int? Score { get; set; }

    public int? MaxScore { get; set; }
}
