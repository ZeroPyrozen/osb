#nullable enable

using System.Text.RegularExpressions;

namespace osb.Helpers;

public static partial class Format
{
    /// <summary>The video ID from a YouTube embed or watch URL, or null if it isn't one.</summary>
    public static string? YouTubeId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;
        var match = YouTubeIdPattern().Match(url);
        return match.Success ? match.Groups["id"].Value : null;
    }

    [GeneratedRegex(@"(?:youtube(?:-nocookie)?\.com/(?:embed/|watch\?v=)|youtu\.be/)(?<id>[A-Za-z0-9_-]{11})")]
    private static partial Regex YouTubeIdPattern();
}
