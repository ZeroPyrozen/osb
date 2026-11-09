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

    /// <summary>Gaps never stop the site, so this test is what stops them reaching main: CI runs it on every pull request.</summary>
    [Fact]
    public void TheRealCourse_HasNoGaps()
    {
        var course = CourseLoader.Load(RepoPaths.Learn);

        Assert.True(course.Gaps.Count == 0, "The learn content has gaps:\n  " + string.Join("\n  ", course.Gaps));
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
        Assert.Equal("Beginner", catalog["paths"]![0]!["difficulty"]!.GetValue<string>());
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

    private static readonly DateOnly Today = new(2026, 11, 9);

    [Fact]
    public void OutcomesTopicsPrerequisitesAndReviewDates_AreRead()
    {
        using var folder = new CourseFolder()
            .Course(["first", "second"], """
                entry: The **basics**, nothing more.
                outcomes:
                  - id: make-scenes
                    bloom: create
                    modules: [first, second]
                    text: Make a scene with `F` and `L`
                """)
            .Module("first", extra: """
                outcomes:
                  - { id: fade, bloom: apply, text: Fade a sprite in with `F` }
                  - { id: move, bloom: Understand, text: Explain what M changes }
                topics:
                  - Fades
                  - The `M` command
                """)
            .Unit("first", "01-check.md", """
                title: Check
                type: quiz
                reviewed: 2026-11-01
                questions:
                  - { prompt: One?, choices: [Yes, No], answer: 0, outcome: fade }
                  - { prompt: Two?, choices: [Yes, No], answer: 1, outcome: fade }
                  - { prompt: Three?, choices: [Yes, No], answer: 0 }
                """)
            .Module("second", extra: """
                prerequisites: [first]
                outcomes:
                  - { id: loop, bloom: apply, text: Write a loop }
                """)
            .Unit("second", "01-exercise.md", """
                title: Exercise
                type: exercise
                outcomes: [loop]
                exercise: { solution: x, checks: [{ kind: parses }] }
                """);

        var course = folder.Load(Today);
        var (first, second) = (course.Modules[0], course.Modules[1]);

        Assert.Equal(["fade", "move"], first.Outcomes.Select(o => o.Id));
        Assert.Equal([BloomLevel.Apply, BloomLevel.Understand], first.Outcomes.Select(o => o.Bloom));
        Assert.Equal("Fade a sprite in with `F`", first.Outcomes[0].Text);
        Assert.Equal("Fade a sprite in with <code>F</code>", first.Outcomes[0].Html);
        Assert.Equal(["Fades", "The <code>M</code> command"], first.Topics);
        Assert.Empty(first.Prerequisites);
        Assert.Equal([first], second.Prerequisites);

        var path = course.Paths[0];
        Assert.Equal("The <strong>basics</strong>, nothing more.", path.Entry);
        var pathOutcome = Assert.Single(path.Outcomes);
        Assert.Equal(("make-scenes", BloomLevel.Create), (pathOutcome.Id, pathOutcome.Bloom));
        Assert.Equal([first, second], pathOutcome.Modules);

        var quiz = first.Units[0];
        Assert.Equal(new string?[] { "fade", "fade", null }, quiz.Quiz!.Questions.Select(q => q.Outcome?.Id));
        Assert.Equal(["fade"], quiz.Assesses.Select(o => o.Id));
        Assert.Equal(new DateOnly(2026, 11, 1), quiz.Reviewed);
        Assert.Equal(["loop"], second.Units[0].Assesses.Select(o => o.Id));
        Assert.Null(second.Units[0].Reviewed);
    }

    [Fact]
    public void TheNewFieldsAreOptional()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics").Lesson("basics", "01-a.md");

        var course = folder.Load(Today);

        Assert.Null(course.Paths[0].Entry);
        Assert.Empty(course.Paths[0].Outcomes);
        Assert.Empty(course.Modules[0].Outcomes);
        Assert.Empty(course.Modules[0].Topics);
        Assert.Empty(course.Units[0].Assesses);
        Assert.Null(course.Units[0].Reviewed);
    }

    [Fact]
    public void OutcomeMistakes_AreReported()
    {
        using var folder = CourseFolder.WithModules("basics")
            .Module("basics", extra: """
                outcomes:
                  - { id: fade, bloom: apply, text: Fade in }
                  - { id: fade, bloom: apply, text: Fade out }
                  - { id: move, bloom: analyze, text: Move }
                  - { id: Big Idea, bloom: create, text: Think }
                  - { id: half, bloom: remember }
                  - { id: built, bloom: apply, text: Build, modules: [basics] }
                  - { id: two, bloom: '2', text: Numbered }
                """)
            .Lesson("basics", "01-a.md");

        string problems = folder.Problems(Today);

        Assert.Contains("basics/module.yml: outcome id 'fade' is used twice.", problems);
        Assert.Contains("basics/module.yml: outcome 'move' has bloom 'analyze'; use remember, understand, apply, analyse, evaluate or create.", problems);
        Assert.Contains("basics/module.yml: outcome id 'Big Idea' must be a lowercase slug, like write-loop.", problems);
        Assert.Contains("basics/module.yml: outcome 5 needs an id, a bloom level and a text.", problems);
        Assert.Contains("basics/module.yml: outcome 'built' lists modules, but only a path's outcomes name the modules that build them.", problems);
        Assert.Contains("basics/module.yml: outcome 'two' has bloom '2';", problems);
    }

    [Fact]
    public void AModuleFileOfTheWrongShape_IsReportedWithItsName()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics", extra: "topics: 5").Lesson("basics", "01-a.md");

        Assert.Contains("basics/module.yml: ", folder.Problems(Today));
    }

    [Fact]
    public void Tags_MustNameAnOutcomeOfTheirOwnModule()
    {
        using var folder = CourseFolder.WithModules("first", "second")
            .Module("first", extra: "outcomes:\n  - { id: fade, bloom: apply, text: Fade in }")
            .Lesson("first", "01-a.md")
            .Module("second", extra: "outcomes:\n  - { id: loop, bloom: apply, text: Write a loop }")
            .Unit("second", "01-check.md", "title: Check\ntype: quiz\nquestions:\n  - { prompt: Yes?, choices: [Yes, No], answer: 0, outcome: fade }")
            .Unit("second", "02-exercise.md", "title: Exercise\ntype: exercise\noutcomes: [loop, nope]\nexercise: { solution: x, checks: [{ kind: parses }] }")
            .Unit("second", "03-lesson.md", "title: Lesson\ntype: lesson\noutcomes: [loop]")
            .Unit("second", "04-quiz.md", "title: Quiz\ntype: quiz\noutcomes: [loop]\nquestions:\n  - { prompt: Yes?, choices: [Yes, No], answer: 0, outcome: loop }");

        string problems = folder.Problems(Today);

        Assert.Contains("second/01-check.md: question 1 names outcome 'fade', which isn't one of this module's outcomes.", problems);
        Assert.Contains("second/02-exercise.md: outcome 'nope' isn't one of this module's outcomes.", problems);
        Assert.Contains("second/03-lesson.md: lessons don't name outcomes; the module's outcomes say what its units teach.", problems);
        Assert.Contains("second/04-quiz.md: a knowledge check names an outcome on each question (outcome:), not in its front matter.", problems);
    }

    [Fact]
    public void Prerequisites_MustBeEarlierModules()
    {
        using var folder = CourseFolder.WithModules("first", "second", "third")
            .Module("first", extra: "prerequisites: [second]").Lesson("first", "01-a.md")
            .Module("second", extra: "prerequisites: [second]").Lesson("second", "01-a.md")
            .Module("third", extra: "prerequisites: [first, ghost]").Lesson("third", "01-a.md");

        string problems = folder.Problems(Today);

        Assert.Contains("first/module.yml: prerequisite 'second' comes later in the course; list only modules that come before this one.", problems);
        Assert.Contains("second/module.yml: a module can't be its own prerequisite.", problems);
        Assert.Contains("third/module.yml: prerequisite 'ghost' isn't a module in course.yml.", problems);
        Assert.DoesNotContain("'first'", problems);
    }

    [Fact]
    public void PathOutcomes_NameOnlyTheirOwnModules_WhilePrerequisitesMaySitInEarlierPaths()
    {
        using var folder = new CourseFolder()
            .File("course.yml", """
                xp: { lesson: 10, quiz: 20, exercise: 30, perfectQuiz: 5, module: 50, path: 150 }
                levels: [{ level: 1, title: Spectator, xp: 0 }]
                achievements: []
                paths:
                  - slug: beginner
                    title: First
                    difficulty: Beginner
                    summary: The basics.
                    trophy: { name: First Light, description: Done. }
                    modules: [basics]
                  - slug: intermediate
                    title: Second
                    difficulty: Intermediate
                    summary: More.
                    trophy: { name: In Sync, description: Done. }
                    modules: [loops]
                    outcomes:
                      - id: loop-well
                        bloom: apply
                        modules: [loops, basics]
                        text: Loop well
                """)
            .Module("basics").Lesson("basics", "01-a.md")
            .Module("loops", extra: "prerequisites: [basics]").Lesson("loops", "01-a.md");

        string problems = folder.Problems(Today);

        Assert.Contains("course.yml, path 'intermediate': outcome 'loop-well' names module 'basics', which isn't in this path.", problems);
        Assert.DoesNotContain("loops/module.yml", problems);
    }

    [Theory]
    [InlineData("2026-13-01", "basics/01-a.md: reviewed must be a date like 2026-11-10 (got '2026-13-01').")]
    [InlineData("'last week'", "basics/01-a.md: reviewed must be a date like 2026-11-10 (got 'last week').")]
    [InlineData("2026-11-11", "basics/01-a.md: reviewed is 2026-11-11, which is in the future.")]
    public void ReviewDates_MustBeRealDates_NotInTheFuture(string reviewed, string problem)
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-a.md", $"title: A\ntype: lesson\nreviewed: {reviewed}");

        Assert.Contains(problem, folder.Problems(Today));
    }

    [Fact]
    public void AReviewDate_MayBeADayAheadOfUtc()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics")
            .Unit("basics", "01-a.md", "title: A\ntype: lesson\nreviewed: 2026-11-10");

        Assert.Equal(new DateOnly(2026, 11, 10), folder.Load(Today).Units[0].Reviewed);
    }

    [Fact]
    public void APathWithoutADifficulty_IsReported_EvenUnderItsOldName()
    {
        using var folder = CourseFolder.WithModules("basics").Module("basics").Lesson("basics", "01-a.md");
        string courseYml = Path.Combine(folder.Root, "course.yml");
        folder.File("course.yml", System.IO.File.ReadAllText(courseYml).Replace("difficulty: Beginner", "level: Beginner"));

        Assert.Contains("course.yml: path 'beginner' needs a difficulty, such as Beginner (it used to be called level).", folder.Problems());
    }

    [Fact]
    public void Gaps_AreListedWithFileNames_ButTheCourseStillLoads()
    {
        using var folder = new CourseFolder()
            .Course(["first", "second"], """
                outcomes:
                  - { id: built, bloom: apply, text: Build, modules: [first] }
                  - { id: unbuilt, bloom: apply, text: Float }
                """)
            .Module("first", extra: """
                outcomes:
                  - { id: fade, bloom: apply, text: Fade in }
                  - { id: move, bloom: apply, text: Move }
                """)
            .Unit("first", "01-check.md", """
                title: Check
                type: quiz
                questions:
                  - { prompt: One?, choices: [Yes, No], answer: 0, outcome: fade }
                  - { prompt: Two?, choices: [Yes, No], answer: 1 }
                """)
            .Unit("first", "02-exercise.md", "title: Exercise\ntype: exercise\nexercise: { solution: x, checks: [{ kind: parses }] }")
            .Module("second", extra: """
                topics: [Loops]
                outcomes:
                  - { id: loop, bloom: apply, text: Loop }
                  - { id: nest, bloom: apply, text: Nest }
                  - { id: stop, bloom: apply, text: Stop }
                """)
            .Unit("second", "01-exercise.md", "title: Exercise\ntype: exercise\noutcomes: [loop, nest, stop]\nexercise: { solution: x, checks: [{ kind: parses }] }");

        var course = folder.Load(Today);

        Assert.Equal([
            "first/module.yml: a module needs 3 to 6 outcomes, and this one has 2.",
            "first/module.yml: the module needs a list of topics.",
            "first/module.yml: no question or exercise in this module assesses outcome 'move'.",
            "first/01-check.md: question 2 doesn't name the outcome it assesses (outcome:).",
            "first/02-exercise.md: the exercise doesn't name the outcomes it assesses (outcomes:).",
            "course.yml, path 'beginner': the path needs an entry requirement (entry:).",
            "course.yml, path 'beginner': a path needs 3 to 6 outcomes, and this one has 2.",
            "course.yml, path 'beginner': outcome 'unbuilt' doesn't name the modules that build it (modules:).",
            "course.yml, path 'beginner': no outcome names module 'second', so it builds none of the path's outcomes.",
        ], course.Gaps);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(6, false)]
    [InlineData(7, true)]
    public void ModulesAndPaths_NeedThreeToSixOutcomes(int count, bool gap)
    {
        var ids = Enumerable.Range(1, count).Select(i => $"o{i}").ToList();
        string Outcomes(string more) => "outcomes:\n" + string.Concat(ids.Select(id => $"  - {{ id: {id}, bloom: apply, text: Do {id}{more} }}\n"));
        using var folder = new CourseFolder()
            .Course(["basics"], "entry: Nothing.\n" + Outcomes(", modules: [basics]"))
            .Module("basics", extra: "topics: [Fades]\n" + Outcomes(""))
            .Unit("basics", "01-exercise.md", $"title: Exercise\ntype: exercise\noutcomes: [{string.Join(", ", ids)}]\nexercise: {{ solution: x, checks: [{{ kind: parses }}] }}");

        var gaps = folder.Load(Today).Gaps;

        string[] expected = gap
            ? [$"basics/module.yml: a module needs 3 to 6 outcomes, and this one has {count}.",
               $"course.yml, path 'beginner': a path needs 3 to 6 outcomes, and this one has {count}."]
            : [];
        Assert.Equal(expected, gaps);
    }
}
