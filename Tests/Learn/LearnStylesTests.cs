using System.Text.Json.Nodes;
using osb.Learn;
using osb.Tests.Infrastructure;

namespace osb.Tests.Learn;

public class LearnStylesTests
{
    [Theory]
    [InlineData("beginner", "text-mint-300", "bg-mint-400")]
    [InlineData("intermediate", "text-gold-300", "bg-gold-400")]
    [InlineData("advanced", "text-[#c5a3ff]", "bg-[#c5a3ff]")]
    [InlineData("a-new-path", "text-mint-300", "bg-mint-400")]
    public void EachPath_HasItsColours(string path, string accent, string bar)
    {
        Assert.Equal(accent, LearnStyles.Accent(path));
        Assert.Equal(bar, LearnStyles.Bar(path));
        Assert.Contains(accent.Replace("text-", "in-data-complete:text-"), LearnStyles.EarnedBadge(path));
    }

    [Fact]
    public void ExerciseJson_AddsTheUnitAndWhereToGoNext()
    {
        using var folder = Exercise("starter: 'Sprite,Foreground,Centre,\"sb/dot.png\",320,240'");
        var unit = folder.Load().Units[0];

        var config = JsonNode.Parse(LearnStyles.ExerciseJson(unit))!;

        Assert.Equal("basics/exercise", config["unitId"]!.GetValue<string>());
        Assert.Equal("Exercise", config["title"]!.GetValue<string>());
        Assert.Equal("/learn/basics/after", config["nextUrl"]!.GetValue<string>());
        Assert.Equal("After", config["nextTitle"]!.GetValue<string>());
        Assert.Equal("Sprite,Foreground,Centre,\"sb/dot.png\",320,240", config["starter"]!.GetValue<string>());
        // The unit's own settings stay as they were, for the next page view.
        Assert.Null(unit.Exercise!["unitId"]);
    }

    [Fact]
    public void ExerciseJson_IsSafeInsideAScriptElement()
    {
        using var folder = Exercise("starter: '// </script><script>alert(1)</script>'");

        string json = LearnStyles.ExerciseJson(folder.Load().Units[0]);

        Assert.DoesNotContain("</script>", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("// </script><script>alert(1)</script>", JsonNode.Parse(json)!["starter"]!.GetValue<string>());
    }

    private static CourseFolder Exercise(string starter) => CourseFolder.WithModules("basics").Module("basics")
        .Unit("basics", "01-exercise.md", $$"""
            title: Exercise
            type: exercise
            exercise:
              {{starter}}
              solution: x
              checks: [{ kind: parses }]
            """)
        .Lesson("basics", "02-after.md", "After");
}
