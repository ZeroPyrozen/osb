using System.Text.RegularExpressions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using osb.Learn;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>
/// What the module page and the path cards show of the outcomes, topics, prerequisites and entry requirements,
/// on a small course: "basics" has none of them and no knowledge check, "loops" has all of them.
/// </summary>
public sealed class LearnPageTests(OsbWebFactory site) : IClassFixture<OsbWebFactory>, IDisposable
{
    private readonly CourseFolder folder = new CourseFolder()
        .Course(["basics", "loops"], """
            entry: the **basics**, nothing more.
            outcomes:
              - { id: loop-well, bloom: apply, text: Loop with `L`, modules: [basics, loops] }
            """)
        .Module("basics")
        .Unit("basics", "01-intro.md", "title: Intro\ntype: lesson\nreviewed: 2026-01-15")
        .Module("loops", extra: """
            prerequisites: [basics]
            topics: [The `L` line, Pass length]
            outcomes:
              - { id: write-loop, bloom: apply, text: Write an `L` loop }
              - { id: count-passes, bloom: understand, text: Count the passes }
            """)
        .Unit("loops", "01-check.md", """
            title: Check
            type: quiz
            questions:
              - { prompt: One?, choices: [Yes, No], answer: 0, outcome: write-loop }
            """);

    public void Dispose() => folder.Dispose();

    [Fact]
    public async Task AModulePage_LinksItsPrerequisites_ForLearnJsToTick()
    {
        string html = await Get("/learn/loops");

        // learn.js sets data-complete on each finished [data-module], which turns its ring into the mint tick.
        string before = Regex.Match(html, "<div[^>]*data-prerequisites[^>]*>.*?</div>", RegexOptions.Singleline).Value;
        Assert.Contains("Before you start", before);
        Assert.Matches("<a href=\"/learn/basics\" data-module=\"basics\"", before);
        Assert.Contains("Module basics<span class=\"sr-only hidden group-data-complete:inline\">, finished</span>", before);
    }

    [Fact]
    public async Task AModulePage_ListsItsOutcomesAndTopics_WithoutBloomLevels()
    {
        string html = await Get("/learn/loops");

        Assert.Contains("By the end of this module, you can", html);
        Assert.Contains("<span>Write an <code>L</code> loop</span>", html);
        Assert.Contains("<span>Count the passes</span>", html);
        Assert.Matches(">The <code>L</code> line</li>\\s*<li[^>]*>Pass length</li>", html);
        Assert.Contains("Knowledge checks wrap up the module", html);
        Assert.DoesNotContain("understand", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("bloom", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AModuleWithoutThem_ShowsNoneOfThoseParts()
    {
        string html = await Get("/learn/basics");

        Assert.DoesNotContain("data-prerequisites", html);
        Assert.DoesNotContain("By the end of this module", html);
        Assert.DoesNotContain("topics-title", html);
        Assert.Contains("Lessons explain an idea", html);
        Assert.DoesNotContain("Knowledge checks wrap up the module", html);
    }

    [Fact]
    public async Task APathCard_ShowsTheEntryRequirement_AndItsOutcomesBehindAClosedToggle()
    {
        string html = await Get("/learn");

        Assert.Contains("Before you start:</strong> the <strong>basics</strong>, nothing more.", html);
        string details = Regex.Match(html, "<details[^>]*data-path-outcomes[^>]*>.*?</details>", RegexOptions.Singleline).Value;
        Assert.StartsWith("<details class=\"group\" data-path-outcomes>", details);
        Assert.Contains("By the end of this path, you can", details);
        Assert.Contains("<span>Loop with <code>L</code></span>", details);
    }

    [Fact]
    public async Task ADatedUnit_SaysWhenItsFactsWereLastChecked()
    {
        string dated = await Get("/learn/basics/intro");
        string undated = await Get("/learn/loops/check");

        // verify-content reads data-reviewed to warn about old dates.
        Assert.Contains("data-reviewed=\"2026-01-15\">Facts last checked on <time datetime=\"2026-01-15\">15 January 2026</time></p>", dated);
        Assert.DoesNotContain("Facts last checked", undated);
    }

    /// <summary>The site, serving the small course above instead of the real one.</summary>
    private async Task<string> Get(string path)
    {
        var courses = new CourseProvider(new TestEnvironment(folder.ContentRoot), NullLogger<CourseProvider>.Instance);
        var client = site.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<CourseProvider>();
            services.AddSingleton(courses);
        })).CreateClient();
        return await client.GetStringAsync(path);
    }
}
