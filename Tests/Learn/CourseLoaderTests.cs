using System.Text.Json;
using System.Text.Json.Nodes;
using osb.Learn;
using osb.Tests.Infrastructure;

namespace osb.Tests.Learn;

public class CourseLoaderTests
{
    [Fact]
    public void TheRealCourse_LoadsWithEveryUnitLinkedInOrder()
    {
        var course = CourseLoader.Load(RepoPaths.Learn);

        Assert.Equal(["beginner", "intermediate", "advanced"], course.Paths.Select(p => p.Slug));
        Assert.All(course.Modules, m => Assert.NotEmpty(m.Units));
        Assert.Equal(Enumerable.Range(1, course.Modules.Count), course.Modules.Select(m => m.Number));
        Assert.Equal(course.Units.Count, course.Units.Select(u => u.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.Null(course.Units[0].Previous);
        Assert.Null(course.Units[^1].Next);
        for (int i = 1; i < course.Units.Count; i++)
        {
            Assert.Same(course.Units[i - 1], course.Units[i].Previous);
            Assert.Same(course.Units[i], course.Units[i - 1].Next);
        }

        // The browser's catalog lists the same units, in the same order.
        using var catalog = JsonDocument.Parse(course.ClientJson);
        Assert.Equal(course.Units.Select(u => u.Id), catalog.RootElement.GetProperty("units").EnumerateArray().Select(u => u.GetProperty("id").GetString()));
    }

    [Fact]
    public void Units_TakeTheirIdOrderAndNumberFromTheFileName()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Lesson("basics", "10-third.md", "Third")
            .Lesson("basics", "02-second.md", "Second")
            .Lesson("basics", "01-first.md", "First");

        var units = folder.Load().Modules[0].Units;

        Assert.Equal(["basics/first", "basics/second", "basics/third"], units.Select(u => u.Id));
        Assert.Equal(["First", "Second", "Third"], units.Select(u => u.Title));
        Assert.Equal([1, 2, 3], units.Select(u => u.Number));
        Assert.Equal("/learn/basics/second", units[1].Url);
        Assert.Equal("/learn/basics", units[1].Module.Url);
    }

    [Fact]
    public void MinutesAndXp_DefaultByUnitType_UnlessTheUnitSetsThem()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-lesson.md", "title: Lesson\ntype: lesson")
            .Unit("basics", "02-quiz.md", "title: Quiz\ntype: quiz\nquestions:\n  - { prompt: Yes?, choices: [Yes, No], answer: 0 }")
            .Unit("basics", "03-exercise.md", "title: Exercise\ntype: exercise\nexercise: { solution: x, checks: [{ kind: parses }] }")
            .Unit("basics", "04-long.md", "title: Long\ntype: lesson\nminutes: 12\nxp: 40");

        var units = folder.Load().Units;

        Assert.Equal([5, 3, 3, 12], units.Select(u => u.Minutes));
        Assert.Equal([10, 20, 30, 40], units.Select(u => u.Xp));
        Assert.Equal([UnitType.Lesson, UnitType.Quiz, UnitType.Exercise, UnitType.Lesson], units.Select(u => u.Type));
    }

    [Fact]
    public void MistakesInTheOutline_AreAllReportedAtOnce_WithFileNames()
    {
        using var folder = new CourseFolder()
            .Course("basics", "basics", "missing", "empty", "odd-icon")
            .Module("basics").Lesson("basics", "01-a.md")
            .Module("empty")
            .Module("odd-icon", icon: "no-such-icon").Lesson("odd-icon", "01-a.md")
            .Module("stray").Lesson("stray", "01-a.md");

        string problems = folder.Problems();

        Assert.Contains("course.yml: module 'basics' is listed more than once.", problems);
        Assert.Contains("course.yml: module 'missing' has no missing/module.yml.", problems);
        Assert.Contains("empty: the module has no units (NN-name.md files).", problems);
        Assert.Contains("odd-icon/module.yml: unknown icon 'no-such-icon' (see Helpers/Icons.cs).", problems);
        Assert.Contains("stray/: this folder isn't listed in any path in course.yml.", problems);
    }

    [Fact]
    public void Units_NeedFrontMatterAKnownTypeAndATitle()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .File("basics/01-plain.md", "Just text, no front matter.")
            .Unit("basics", "02-video.md", "title: A video\ntype: video")
            .Unit("basics", "03-untitled.md", "type: lesson")
            .Unit("basics", "04-broken.md", "title: [unclosed");

        string problems = folder.Problems();

        Assert.Contains("basics/01-plain.md: missing the --- YAML front matter --- block (title, type, minutes).", problems);
        Assert.Contains("basics/02-video.md: type must be lesson, quiz or exercise (got 'video').", problems);
        Assert.Contains("basics/03-untitled.md: missing a title.", problems);
        Assert.Contains("basics/04-broken.md: ", problems);
    }

    [Fact]
    public void Quizzes_AreChecked()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-empty.md", "title: Empty\ntype: quiz")
            .Unit("basics", "02-mistakes.md", """
                title: Mistakes
                type: quiz
                pass: 5
                questions:
                  - { prompt: One choice, choices: [Only], answer: 0 }
                  - { prompt: Out of range, choices: [A, B, C], answer: 3 }
                """);

        string problems = folder.Problems();

        Assert.Contains("basics/01-empty.md: a quiz needs a list of questions.", problems);
        Assert.Contains("basics/02-mistakes.md: question 1 needs at least two choices.", problems);
        Assert.Contains("basics/02-mistakes.md: question 2 has answer 3, but choices are numbered 0 to 2.", problems);
        Assert.Contains("basics/02-mistakes.md: pass must be between 1 and the number of questions (2).", problems);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(4, 3)]
    [InlineData(5, 3)]
    [InlineData(7, 5)]
    public void QuizPassMark_DefaultsToSixtyPercent_RoundedUp(int questions, int pass)
    {
        string list = string.Concat(Enumerable.Range(1, questions).Select(i => $"\n  - {{ prompt: Q{i}, choices: [A, B], answer: 1 }}"));
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-quiz.md", "title: Quiz\ntype: quiz\nquestions:" + list);

        var quiz = folder.Load().Units[0].Quiz!;

        Assert.Equal(questions, quiz.Questions.Count);
        Assert.Equal(pass, quiz.Pass);
    }

    [Fact]
    public void QuizText_IsMarkdown_PromptsAndChoicesWithoutParagraphs()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-quiz.md", """
                title: Quiz
                type: quiz
                questions:
                  - prompt: Which command uses `F`?
                    choices: ['**Fade**', Move]
                    answer: 0
                    explanation: '`F` changes the opacity.'
                """);

        var question = folder.Load().Units[0].Quiz!.Questions[0];

        Assert.Equal("Which command uses <code>F</code>?", question.Prompt);
        Assert.Equal(["<strong>Fade</strong>", "Move"], question.Choices);
        Assert.Equal(0, question.Answer);
        Assert.Equal("<p><code>F</code> changes the opacity.</p>", question.Explanation.Trim());
    }

    [Fact]
    public void Exercises_AreChecked()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-none.md", "title: None\ntype: exercise")
            .Unit("basics", "02-mode.md", "title: Mode\ntype: exercise\nexercise: { mode: wasm, solution: x, checks: [{ kind: parses }] }")
            .Unit("basics", "03-unsolved.md", "title: Unsolved\ntype: exercise\nexercise: { checks: [{ kind: parses }] }")
            .Unit("basics", "04-unchecked.md", "title: Unchecked\ntype: exercise\nexercise: { solution: x, checks: [] }")
            .Unit("basics", "05-unknown.md", "title: Unknown\ntype: exercise\nexercise: { solution: x, checks: [{ kind: colour }] }");

        string problems = folder.Problems();

        Assert.Contains("basics/01-none.md: an exercise needs an exercise: block (starter, solution, checks).", problems);
        Assert.Contains("basics/02-mode.md: exercise mode must be osb or script.", problems);
        Assert.Contains("basics/03-unsolved.md: the exercise needs a solution.", problems);
        Assert.Contains("basics/04-unchecked.md: the exercise needs at least one check.", problems);
        Assert.Contains("basics/05-unknown.md: unknown check kind 'colour'. Known kinds: ", problems);
    }

    [Fact]
    public void Exercises_DefaultToOsbModeAndAnEmptyStarter()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-exercise.md", "title: Exercise\ntype: exercise\nexercise: { solution: x, checks: [{ kind: parses }] }");

        var exercise = folder.Load().Units[0].Exercise!;

        Assert.Equal("osb", exercise["mode"]!.GetValue<string>());
        Assert.Equal("", exercise["starter"]!.GetValue<string>());
    }

    /// <summary>Starter code and expected values must reach the browser exactly as written.</summary>
    [Fact]
    public void Yaml_PlainValuesGetTypes_QuotedAndBlockValuesStayText()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-exercise.md", """
                title: Exercise
                type: exercise
                exercise:
                  starter: |
                    1,2
                  solution: x
                  checks:
                    - { kind: value, at: 1000, equals: '1', within: 0.5, exact: true, label: ~ }
                """);

        var exercise = folder.Load().Units[0].Exercise!;
        var check = (JsonObject)exercise["checks"]![0]!;

        Assert.Equal("1,2\n", exercise["starter"]!.GetValue<string>());
        Assert.Equal(1000L, check["at"]!.GetValue<long>());
        Assert.Equal("1", check["equals"]!.GetValue<string>());
        Assert.Equal(0.5, check["within"]!.GetValue<double>());
        Assert.True(check["exact"]!.GetValue<bool>());
        Assert.Null(check["label"]);
    }

    [Fact]
    public void Lessons_ListTheirSecondLevelHeadings_ForOnThisPage()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Lesson("basics", "01-a.md", body: "## Fading in\n\nText.\n\n### A detail\n\nMore.\n\n## Use `F` here\n\nEnd.");

        var unit = folder.Load().Units[0];

        Assert.Equal([("fading-in", "Fading in"), ("use-f-here", "Use F here")], unit.Headings);
        Assert.Contains("<h2 id=\"fading-in\">Fading in</h2>", unit.Html);
    }

    [Fact]
    public void Levels_AreSortedByXp()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics").Lesson("basics", "01-a.md");

        Assert.Equal([1, 2], folder.Load().Levels.Select(l => l.Level));
    }

    [Fact]
    public void TheCatalog_HasWhatTheBrowserNeeds()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Lesson("basics", "01-a.md", "First")
            .Unit("basics", "02-quiz.md", "title: Check\ntype: quiz\nquestions:\n  - { prompt: Yes?, choices: [Yes, No], answer: 0 }");

        var catalog = JsonNode.Parse(folder.Load().ClientJson)!;

        Assert.Equal("""{"id":"basics/a","module":"basics","type":"lesson","xp":10,"title":"First","url":"/learn/basics/a"}""", catalog["units"]![0]!.ToJsonString());
        Assert.Equal("quiz", catalog["units"]![1]!["type"]!.GetValue<string>());
        Assert.Equal(50, catalog["modules"]![0]!["bonus"]!.GetValue<int>());
        Assert.Equal(150, catalog["paths"]![0]!["bonus"]!.GetValue<int>());
        Assert.Equal([0, 60], catalog["levels"]!.AsArray().Select(l => l!["xp"]!.GetValue<int>()));
        Assert.Equal(5, catalog["quiz"]!["perfectBonus"]!.GetValue<int>());
    }

    [Fact]
    public void TheCatalog_IsSafeInsideAScriptElement()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-a.md", "title: '</script><script>alert(1)</script>'\ntype: lesson");

        string json = folder.Load().ClientJson;

        Assert.DoesNotContain("</script>", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\\u003C/script\\u003E", json);
    }

    [Fact]
    public void AMissingCourseFile_IsReported()
    {
        using var folder = new CourseFolder().Module("basics");

        Assert.Contains("course.yml is missing.", folder.Problems());
    }
}
