#nullable enable

using System.Globalization;
using System.Text.RegularExpressions;

namespace osb.Helpers;

/// <summary>A beatmapset ID, or a beatmap (one difficulty) ID when <see cref="IsDifficulty"/> is set.</summary>
public readonly record struct BeatmapLink(int Id, bool IsDifficulty);

/// <summary>Reads the osu! links, IDs and names people paste into forms.</summary>
public static partial class OsuLinks
{
    /// <summary>
    /// What a link points at: osu.ppy.sh/beatmapsets/123 (also with #osu/456 or /discussion after it)
    /// and osu.ppy.sh/s/123 are beatmapsets, osu.ppy.sh/beatmaps/456 and osu.ppy.sh/b/456 difficulties.
    /// A plain number is a beatmapset ID. Anything else is null.
    /// </summary>
    public static BeatmapLink? ParseBeatmap(string? input)
    {
        string text = input?.Trim() ?? "";
        if (PositiveNumber(text) is int id)
            return new BeatmapLink(id, IsDifficulty: false);

        var match = BeatmapUrl().Match(text);
        if (!match.Success || PositiveNumber(match.Groups["id"].Value) is not int linked)
            return null;
        return new BeatmapLink(linked, IsDifficulty: match.Groups["kind"].Value.ToLowerInvariant() is "beatmaps" or "b");
    }

    /// <summary>
    /// The people in a list such as "Alice, osu.ppy.sh/users/2, 727", in order and each once. Commas
    /// and new lines separate them, since osu! usernames can't contain either. A profile link becomes
    /// the ID or name in it, and a leading @ is dropped.
    /// </summary>
    public static IReadOnlyList<string> ParseUsers(string? input)
    {
        var people = new List<string>();
        foreach (string part in (input ?? "").Split([',', '\n', '\r'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var profile = ProfileUrl().Match(part);
            string person = profile.Success ? Uri.UnescapeDataString(profile.Groups["user"].Value).Trim() : part.TrimStart('@').Trim();
            if (person.Length > 0 && !people.Contains(person, StringComparer.OrdinalIgnoreCase))
                people.Add(person);
        }
        return people;
    }

    private static int? PositiveNumber(string text) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int number) && number > 0 ? number : null;

    [GeneratedRegex(@"^(?:https?://)?(?:osu|old)\.ppy\.sh/(?<kind>beatmapsets|s|beatmaps|b)/(?<id>\d+)(?:[/?#].*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex BeatmapUrl();

    [GeneratedRegex(@"^(?:https?://)?(?:osu|old)\.ppy\.sh/(?:users|u)/(?<user>[^/?#]+)(?:[/?#].*)?$", RegexOptions.IgnoreCase)]
    private static partial Regex ProfileUrl();
}
