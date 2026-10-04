#nullable enable

using System.Text.Json.Nodes;

namespace osb.Learn;

public enum UnitType { Lesson, Quiz, Exercise }

/// <summary>The whole course: paths, their modules and units, plus the XP and level rules.</summary>
public sealed class Course
{
    public required IReadOnlyList<LearnPath> Paths { get; init; }
    public required IReadOnlyList<LearnModule> Modules { get; init; }
    public required IReadOnlyList<LearnUnit> Units { get; init; }
    public required IReadOnlyList<LevelRule> Levels { get; init; }
    public required IReadOnlyList<Achievement> Achievements { get; init; }
    public required XpRules Xp { get; init; }

    /// <summary>The catalog as the browser sees it (see Scripts/learn/gamification.js), serialized once.</summary>
    public required string ClientJson { get; init; }

    public int TotalMinutes => Units.Sum(u => u.Minutes);

    public LearnModule? FindModule(string slug) =>
        Modules.FirstOrDefault(m => string.Equals(m.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public LearnUnit? FindUnit(string id) =>
        Units.FirstOrDefault(u => string.Equals(u.Id, id, StringComparison.OrdinalIgnoreCase));
}

public sealed record LevelRule(int Level, string Title, int Xp);

public sealed record Achievement(string Id, string Name, string Description);

public sealed record Award(string Name, string Description);

public sealed record XpRules(int Lesson, int Quiz, int Exercise, int Module, int Path, int PerfectQuiz)
{
    public int For(UnitType type) => type switch
    {
        UnitType.Quiz => Quiz,
        UnitType.Exercise => Exercise,
        _ => Lesson,
    };
}

/// <summary>A learning path, e.g. "Beginner: Your first storyboard".</summary>
public sealed class LearnPath
{
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Level { get; init; }
    public required string Summary { get; init; }
    public required Award Trophy { get; init; }
    public List<LearnModule> Modules { get; } = new();

    public int Minutes => Modules.Sum(m => m.Minutes);
    public int UnitCount => Modules.Sum(m => m.Units.Count);
}

/// <summary>A module: a handful of units on one topic, ending in a badge.</summary>
public sealed class LearnModule
{
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Icon { get; init; }
    public required Award Badge { get; init; }
    public required LearnPath Path { get; init; }
    public List<LearnUnit> Units { get; } = new();

    public string Url => $"/learn/{Slug}";
    public int Minutes => Units.Sum(u => u.Minutes);
    public int Number { get; set; }
}

/// <summary>One page of the course: a lesson, a knowledge check or an exercise.</summary>
public sealed class LearnUnit
{
    /// <summary><c>module-slug/unit-slug</c>; also the key stored in learner progress.</summary>
    public required string Id { get; init; }
    public required string Slug { get; init; }
    public required LearnModule Module { get; init; }
    public required string Title { get; init; }
    public string? Summary { get; init; }
    public required UnitType Type { get; init; }
    public required int Minutes { get; init; }
    public required int Xp { get; init; }

    /// <summary>The unit's Markdown body rendered to HTML.</summary>
    public required string Html { get; init; }

    /// <summary>Level-2 headings, for the "On this page" list.</summary>
    public required IReadOnlyList<(string Id, string Text)> Headings { get; init; }

    public Quiz? Quiz { get; init; }

    /// <summary>The exercise configuration from the front matter, passed to the browser as JSON.</summary>
    public JsonObject? Exercise { get; init; }

    public string Url => $"/learn/{Module.Slug}/{Slug}";
    public int Number { get; set; }
    public LearnUnit? Previous { get; set; }
    public LearnUnit? Next { get; set; }

    public string TypeLabel => Type switch
    {
        UnitType.Quiz => "Knowledge check",
        UnitType.Exercise => "Exercise",
        _ => "Lesson",
    };

    public string TypeIcon => Type switch
    {
        UnitType.Quiz => "quiz",
        UnitType.Exercise => "code",
        _ => "book",
    };
}

public sealed record Quiz(int Pass, IReadOnlyList<QuizQuestion> Questions);

/// <summary>A single-choice question. Prompt, choices and explanation are HTML rendered from Markdown.</summary>
public sealed record QuizQuestion(string Prompt, IReadOnlyList<string> Choices, int Answer, string Explanation);
