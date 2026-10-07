using System.Text.RegularExpressions;
using osb.Helpers;
using osb.Tests.Infrastructure;

namespace osb.Tests.Helpers;

public partial class IconsTests
{
    [Fact]
    public void Get_DrawsAnInlineSvgWithTheGivenClasses()
    {
        string svg = Icons.Get("search", "size-5 text-sage").ToString()!;

        Assert.StartsWith("<svg class=\"size-5 text-sage\" viewBox=\"0 0 19 19\"", svg);
        Assert.Contains("aria-hidden=\"true\"", svg);
        Assert.EndsWith("</svg>", svg);
    }

    [Fact]
    public void Get_RejectsUnknownNames() =>
        Assert.Throws<ArgumentException>(() => Icons.Get("no-such-icon"));

    /// <summary>An unknown name throws while the page renders, so check every name the site uses.</summary>
    [Fact]
    public void EveryIconTheSiteUses_Exists()
    {
        var files = Directory.EnumerateFiles(Path.Combine(RepoPaths.Root, "Views"), "*.cshtml", SearchOption.AllDirectories);
        var used = files
            .SelectMany(file => IconCall().Matches(File.ReadAllText(file)).Select(m => (File: Path.GetFileName(file), Name: m.Groups[1].Value)))
            .ToList();

        Assert.NotEmpty(used);
        Assert.All(used, use => Assert.True(Icons.Exists(use.Name), $"{use.File} uses the unknown icon '{use.Name}'."));
    }

    [GeneratedRegex(@"Icons\.Get\(""([^""]+)""")]
    private static partial Regex IconCall();
}
