#nullable enable

using System.Text.Json;
using System.Text.Json.Nodes;

namespace osb.Learn;

/// <summary>Colours per path, so modules and badges of a path look like they belong together.</summary>
public static class LearnStyles
{
    /// <summary>Accent text colour for a path: mint for beginner, gold for intermediate, violet for advanced.</summary>
    public static string Accent(string pathSlug) => pathSlug switch
    {
        "intermediate" => "text-gold-300",
        "advanced" => "text-[#c5a3ff]",
        _ => "text-mint-300",
    };

    /// <summary>Accent background for progress bars.</summary>
    public static string Bar(string pathSlug) => pathSlug switch
    {
        "intermediate" => "bg-gold-400",
        "advanced" => "bg-[#c5a3ff]",
        _ => "bg-mint-400",
    };

    /// <summary>
    /// Colours a badge once its module is complete (an ancestor gets data-complete).
    /// Written out in full because Tailwind only generates class names it can find in the source.
    /// </summary>
    public static string EarnedBadge(string pathSlug) => pathSlug switch
    {
        "intermediate" => "in-data-complete:border-gold-300 in-data-complete:text-gold-300",
        "advanced" => "in-data-complete:border-[#c5a3ff] in-data-complete:text-[#c5a3ff]",
        _ => "in-data-complete:border-mint-300 in-data-complete:text-mint-300",
    };

    /// <summary>The exercise configuration for the page, with the unit's id and the next unit to go to.</summary>
    public static string ExerciseJson(LearnUnit unit)
    {
        var config = (JsonObject)unit.Exercise!.DeepClone();
        config["unitId"] = unit.Id;
        config["title"] = unit.Title;
        config["nextUrl"] = unit.Next?.Url;
        config["nextTitle"] = unit.Next?.Title;
        // The default encoder escapes <, > and &, so the JSON is safe inside a <script> element.
        return config.ToJsonString(new JsonSerializerOptions());
    }
}
