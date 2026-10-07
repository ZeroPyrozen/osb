#nullable enable

using System.Text.RegularExpressions;

namespace osb.Helpers;

public static partial class Format
{
    /// <summary>The video ID from a YouTube link (watch, embed, shorts, live or youtu.be), or null if it isn't one.</summary>
    public static string? YouTubeId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;
        var match = YouTubeIdPattern().Match(url);
        return match.Success ? match.Groups["id"].Value : null;
    }

    /// <summary>The embed link the site stores and plays for a YouTube link, or null if it isn't one.</summary>
    public static string? YouTubeEmbedUrl(string? url) =>
        YouTubeId(url) is { } id ? $"https://www.youtube.com/embed/{id}" : null;

    [GeneratedRegex(@"(?:youtube(?:-nocookie)?\.com/(?:embed/|shorts/|live/|watch\?(?:[^#\s]*&)?v=)|youtu\.be/)(?<id>[A-Za-z0-9_-]{11})(?![A-Za-z0-9_-])")]
    private static partial Regex YouTubeIdPattern();
}
