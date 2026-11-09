#nullable enable

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using osb.Helpers;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace osb.Learn;

/// <summary>
/// Reads the course from <c>Content/Learn</c>:
/// <list type="bullet">
///   <item><c>course.yml</c>: XP rules, levels, achievements and the paths: their module order, entry requirement and outcomes.</item>
///   <item><c>&lt;module&gt;/module.yml</c>: a module's title, summary, icon, badge, outcomes, topics and prerequisites.</item>
///   <item><c>&lt;module&gt;/NN-&lt;unit&gt;.md</c>: units in file-name order, Markdown with YAML front matter.</item>
/// </list>
/// Everything is validated up front. Mistakes, which make the content inconsistent, are reported together
/// with file names and stop the load. Gaps, which only leave it incomplete, come back in <see cref="Course.Gaps"/>.
/// </summary>
public static partial class CourseLoader
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .UseAdvancedExtensions()
        .Build();

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Check kinds understood by Scripts/storyboard/checks.js.</summary>
    private static readonly HashSet<string> CheckKinds =
        ["parses", "count", "object", "command", "value", "visible", "lifetime", "matches-solution"];

    /// <summary>A module or path is complete with this many outcomes.</summary>
    private const int FewestOutcomes = 3, MostOutcomes = 6;

    /// <param name="root">The course folder, with course.yml.</param>
    /// <param name="today">Today's date, for checking review dates; defaults to today in UTC.</param>
    public static Course Load(string root, DateOnly? today = null)
    {
        var errors = new List<string>();
        var gaps = new List<string>();
        string Rel(string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

        // A review date may be a day ahead of UTC: it's the author's own date, and osb is east of Greenwich.
        var latestReview = (today ?? DateOnly.FromDateTime(DateTime.UtcNow)).AddDays(1);

        var config = ReadYaml(Path.Combine(root, "course.yml")).Deserialize<CourseFile>(Json)
            ?? throw new InvalidDataException("course.yml is empty.");

        var paths = new List<LearnPath>();
        var modules = new List<LearnModule>();
        var units = new List<LearnUnit>();
        var seenModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var courseOrder = config.Paths.SelectMany(p => p.Modules).ToList();

        foreach (var pathFile in config.Paths)
        {
            if (string.IsNullOrWhiteSpace(pathFile.Difficulty))
                errors.Add($"course.yml: path '{pathFile.Slug}' needs a difficulty, such as Beginner (it used to be called level).");

            var path = new LearnPath
            {
                Slug = pathFile.Slug,
                Title = pathFile.Title,
                Difficulty = pathFile.Difficulty,
                Summary = pathFile.Summary,
                Trophy = pathFile.Trophy,
                Entry = string.IsNullOrWhiteSpace(pathFile.Entry) ? null : InlineHtml(pathFile.Entry.Trim()),
            };
            paths.Add(path);

            foreach (string slug in pathFile.Modules)
            {
                if (!seenModules.Add(slug))
                {
                    errors.Add($"course.yml: module '{slug}' is listed more than once.");
                    continue;
                }
                string folder = Path.Combine(root, slug);
                string moduleYml = Path.Combine(folder, "module.yml");
                if (!File.Exists(moduleYml))
                {
                    errors.Add($"course.yml: module '{slug}' has no {slug}/module.yml.");
                    continue;
                }

                ModuleFile moduleFile;
                try
                {
                    moduleFile = ReadYaml(moduleYml).Deserialize<ModuleFile>(Json)!;
                }
                catch (Exception e) when (e is YamlException or JsonException or InvalidDataException)
                {
                    errors.Add($"{slug}/module.yml: {e.Message}");
                    continue;
                }

                string where = $"{slug}/module.yml";
                if (!Icons.Exists(moduleFile.Icon))
                    errors.Add($"{where}: unknown icon '{moduleFile.Icon}' (see Helpers/Icons.cs).");

                var outcomes = new List<Outcome>();
                foreach (var (outcomeFile, outcome) in ReadOutcomes(moduleFile.Outcomes, where, errors))
                {
                    if (outcomeFile.Modules != null)
                        errors.Add($"{where}: outcome '{outcome.Id}' lists modules, but only a path's outcomes name the modules that build them.");
                    outcomes.Add(outcome);
                }

                var module = new LearnModule
                {
                    Slug = slug,
                    Title = moduleFile.Title,
                    Summary = moduleFile.Summary,
                    Icon = moduleFile.Icon,
                    Badge = moduleFile.Badge,
                    Path = path,
                    Number = modules.Count + 1,
                    Outcomes = outcomes,
                    Topics = (moduleFile.Topics ?? []).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => InlineHtml(t.Trim())).ToList(),
                };
                // Before the module joins the list, so it holds exactly the modules that come earlier.
                LinkPrerequisites(module, moduleFile.Prerequisites, modules, courseOrder, errors);
                path.Modules.Add(module);
                modules.Add(module);

                var unitGaps = new List<string>();
                foreach (string file in Directory.GetFiles(folder, "*.md").OrderBy(f => f, StringComparer.Ordinal))
                {
                    try
                    {
                        var unit = ReadUnit(file, module, config.Xp, latestReview, errors, Rel(file));
                        if (unit == null)
                            continue;
                        unit.Number = module.Units.Count + 1;
                        module.Units.Add(unit);
                        units.Add(unit);
                        unitGaps.AddRange(UnitGaps(unit, Rel(file)));
                    }
                    catch (Exception e) when (e is YamlException or JsonException or InvalidDataException)
                    {
                        errors.Add($"{Rel(file)}: {e.Message}");
                    }
                }
                if (module.Units.Count == 0)
                    errors.Add($"{slug}: the module has no units (NN-name.md files).");
                // The module's own gaps first, then its units' in file order.
                gaps.AddRange(ModuleGaps(module, where));
                gaps.AddRange(unitGaps);
            }

            // After the path's modules are read, so each outcome can name the ones that build it.
            string pathWhere = $"course.yml, path '{pathFile.Slug}'";
            foreach (var (outcomeFile, outcome) in ReadOutcomes(pathFile.Outcomes, pathWhere, errors))
            {
                var builtBy = new List<LearnModule>();
                foreach (string name in outcomeFile.Modules ?? [])
                {
                    var match = path.Modules.FirstOrDefault(m => string.Equals(m.Slug, name, StringComparison.OrdinalIgnoreCase));
                    if (match == null)
                        errors.Add($"{pathWhere}: outcome '{outcome.Id}' names module '{name}', which isn't in this path.");
                    else if (!builtBy.Contains(match))
                        builtBy.Add(match);
                }
                path.Outcomes.Add(new PathOutcome(outcome.Id, outcome.Bloom, outcome.Text, outcome.Html, builtBy));
            }
            gaps.AddRange(PathGaps(path, pathWhere));
        }

        foreach (string folder in Directory.GetDirectories(root))
        {
            string name = Path.GetFileName(folder);
            if (!seenModules.Contains(name))
                errors.Add($"{name}/: this folder isn't listed in any path in course.yml.");
        }

        if (errors.Count > 0)
            throw new InvalidDataException("The learn content has problems:\n  " + string.Join("\n  ", errors));

        for (int i = 0; i < units.Count; i++)
        {
            units[i].Previous = i > 0 ? units[i - 1] : null;
            units[i].Next = i + 1 < units.Count ? units[i + 1] : null;
        }

        return new Course
        {
            Paths = paths,
            Modules = modules,
            Units = units,
            Levels = config.Levels.OrderBy(l => l.Xp).ToList(),
            Achievements = config.Achievements,
            Xp = config.Xp,
            ClientJson = BuildClientJson(paths, modules, units, config),
            Gaps = gaps,
        };
    }

    /// <summary>What a module still lacks: 3 to 6 outcomes, its topics, and a question or exercise for every outcome.</summary>
    private static IEnumerable<string> ModuleGaps(LearnModule module, string where)
    {
        if (module.Outcomes.Count is < FewestOutcomes or > MostOutcomes)
            yield return $"{where}: a module needs {FewestOutcomes} to {MostOutcomes} outcomes, and this one has {module.Outcomes.Count}.";
        if (module.Topics.Count == 0)
            yield return $"{where}: the module needs a list of topics.";
        foreach (var outcome in module.Outcomes.Where(o => !module.Units.Any(u => u.Assesses.Contains(o))))
            yield return $"{where}: no question or exercise in this module assesses outcome '{outcome.Id}'.";
    }

    /// <summary>What a unit still lacks: every question of a knowledge check, and every exercise, names what it assesses.</summary>
    private static IEnumerable<string> UnitGaps(LearnUnit unit, string name)
    {
        var questions = unit.Quiz?.Questions ?? [];
        for (int i = 0; i < questions.Count; i++)
        {
            if (questions[i].Outcome == null)
                yield return $"{name}: question {i + 1} doesn't name the outcome it assesses (outcome:).";
        }
        if (unit.Type == UnitType.Exercise && unit.Assesses.Count == 0)
            yield return $"{name}: the exercise doesn't name the outcomes it assesses (outcomes:).";
    }

    /// <summary>
    /// What a path still lacks: an entry requirement, and 3 to 6 outcomes that each name the modules building them,
    /// so that every module builds at least one.
    /// </summary>
    private static IEnumerable<string> PathGaps(LearnPath path, string where)
    {
        if (path.Entry == null)
            yield return $"{where}: the path needs an entry requirement (entry:).";
        if (path.Outcomes.Count is < FewestOutcomes or > MostOutcomes)
            yield return $"{where}: a path needs {FewestOutcomes} to {MostOutcomes} outcomes, and this one has {path.Outcomes.Count}.";
        foreach (var outcome in path.Outcomes.Where(o => o.Modules.Count == 0))
            yield return $"{where}: outcome '{outcome.Id}' doesn't name the modules that build it (modules:).";
        foreach (var module in path.Modules.Where(m => !path.Outcomes.Any(o => o.Modules.Contains(m))))
            yield return $"{where}: no outcome names module '{module.Slug}', so it builds none of the path's outcomes.";
    }

    private static LearnUnit? ReadUnit(string file, LearnModule module, XpRules xp, DateOnly latestReview, List<string> errors, string name)
    {
        string text = File.ReadAllText(file);
        var document = Markdown.Parse(text, Pipeline);
        var frontMatter = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (frontMatter == null)
        {
            errors.Add($"{name}: missing the --- YAML front matter --- block (title, type, minutes).");
            return null;
        }

        string yaml = string.Join('\n', frontMatter.Lines.Lines.Take(frontMatter.Lines.Count).Select(l => l.ToString()));
        var meta = ParseYaml(yaml).Deserialize<UnitFile>(Json)
            ?? throw new InvalidDataException("the front matter is empty.");

        if (!Enum.TryParse<UnitType>(meta.Type, ignoreCase: true, out var type))
        {
            errors.Add($"{name}: type must be lesson, quiz or exercise (got '{meta.Type}').");
            return null;
        }
        if (string.IsNullOrWhiteSpace(meta.Title))
            errors.Add($"{name}: missing a title.");

        string slug = UnitSlugPattern().Replace(Path.GetFileNameWithoutExtension(file), "");
        Quiz? quiz = type == UnitType.Quiz ? ReadQuiz(meta, module.Outcomes, errors, name) : null;
        JsonObject? exercise = type == UnitType.Exercise ? ReadExercise(meta.Exercise, errors, name) : null;

        return new LearnUnit
        {
            Id = $"{module.Slug}/{slug}",
            Slug = slug,
            Module = module,
            Title = meta.Title,
            Summary = meta.Summary,
            Type = type,
            Minutes = meta.Minutes ?? (type == UnitType.Lesson ? 5 : 3),
            Xp = meta.Xp ?? xp.For(type),
            Html = document.ToHtml(Pipeline),
            Headings = document.Descendants<HeadingBlock>()
                .Where(h => h.Level == 2 && h.GetAttributes().Id != null)
                .Select(h => (h.GetAttributes().Id!, PlainText(h.Inline)))
                .ToList(),
            Quiz = quiz,
            Exercise = exercise,
            Assesses = ReadAssesses(type, meta, quiz, module.Outcomes, errors, name),
            Reviewed = ReadReviewDate(meta.Reviewed, latestReview, errors, name),
        };
    }

    private static Quiz? ReadQuiz(UnitFile meta, IReadOnlyList<Outcome> outcomes, List<string> errors, string name)
    {
        if (meta.Questions is not { Count: > 0 })
        {
            errors.Add($"{name}: a quiz needs a list of questions.");
            return null;
        }
        var questions = new List<QuizQuestion>();
        for (int i = 0; i < meta.Questions.Count; i++)
        {
            var q = meta.Questions[i];
            if (q.Choices is not { Count: >= 2 })
                errors.Add($"{name}: question {i + 1} needs at least two choices.");
            else if (q.Answer < 0 || q.Answer >= q.Choices.Count)
                errors.Add($"{name}: question {i + 1} has answer {q.Answer}, but choices are numbered 0 to {q.Choices.Count - 1}.");
            Outcome? outcome = null;
            if (q.Outcome != null)
            {
                outcome = outcomes.FirstOrDefault(o => o.Id == q.Outcome);
                if (outcome == null)
                    errors.Add($"{name}: question {i + 1} names outcome '{q.Outcome}', which isn't one of this module's outcomes.");
            }
            questions.Add(new QuizQuestion(
                InlineHtml(q.Prompt),
                (q.Choices ?? new()).Select(InlineHtml).ToList(),
                q.Answer,
                q.Explanation != null ? Markdown.ToHtml(q.Explanation, Pipeline) : "",
                outcome));
        }
        int pass = meta.Pass ?? (int)Math.Ceiling(questions.Count * 0.6);
        if (pass < 1 || pass > questions.Count)
            errors.Add($"{name}: pass must be between 1 and the number of questions ({questions.Count}).");
        return new Quiz(pass, questions);
    }

    private static JsonObject? ReadExercise(JsonObject? exercise, List<string> errors, string name)
    {
        if (exercise == null)
        {
            errors.Add($"{name}: an exercise needs an exercise: block (starter, solution, checks).");
            return null;
        }
        string mode = exercise["mode"]?.GetValue<string>() ?? "osb";
        if (mode is not ("osb" or "script"))
            errors.Add($"{name}: exercise mode must be osb or script.");
        if (exercise["solution"] is not JsonValue solution || string.IsNullOrWhiteSpace(solution.GetValue<string>()))
            errors.Add($"{name}: the exercise needs a solution.");
        if (exercise["checks"] is not JsonArray { Count: > 0 } checks)
        {
            errors.Add($"{name}: the exercise needs at least one check.");
        }
        else
        {
            foreach (var check in checks)
            {
                string? kind = check?["kind"]?.GetValue<string>();
                if (kind == null || !CheckKinds.Contains(kind))
                    errors.Add($"{name}: unknown check kind '{kind}'. Known kinds: {string.Join(", ", CheckKinds)}.");
            }
        }
        exercise["mode"] = mode;
        exercise["starter"] ??= "";
        return exercise;
    }

    /// <summary>A module's or path's outcomes, checked, each with its entry in the file (a path outcome's names its modules).</summary>
    private static List<(OutcomeFile File, Outcome Outcome)> ReadOutcomes(List<OutcomeFile?>? files, string where, List<string> errors)
    {
        var outcomes = new List<(OutcomeFile, Outcome)>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < (files?.Count ?? 0); i++)
        {
            var file = files![i];
            if (file == null || string.IsNullOrWhiteSpace(file.Id) || string.IsNullOrWhiteSpace(file.Bloom) || string.IsNullOrWhiteSpace(file.Text))
            {
                errors.Add($"{where}: outcome {i + 1} needs an id, a bloom level and a text.");
                continue;
            }
            if (!OutcomeIdPattern().IsMatch(file.Id))
                errors.Add($"{where}: outcome id '{file.Id}' must be a lowercase slug, like write-loop.");
            else if (!ids.Add(file.Id))
                errors.Add($"{where}: outcome id '{file.Id}' is used twice.");
            if (!TryParseBloom(file.Bloom, out var bloom))
            {
                errors.Add($"{where}: outcome '{file.Id}' has bloom '{file.Bloom}'; use remember, understand, apply, analyse, evaluate or create.");
                continue;
            }
            string text = file.Text.Trim();
            outcomes.Add((file, new Outcome(file.Id, bloom, text, InlineHtml(text))));
        }
        return outcomes;
    }

    /// <summary>One of the six level names, in any case. Enum.TryParse alone would also take numbers and comma lists.</summary>
    private static bool TryParseBloom(string value, out BloomLevel bloom) =>
        Enum.TryParse(value.Trim(), ignoreCase: true, out bloom) && value.Trim().All(char.IsLetter);

    /// <summary>Links a module to the earlier modules it builds on; <paramref name="earlier"/> holds every module before it.</summary>
    private static void LinkPrerequisites(LearnModule module, List<string>? names, List<LearnModule> earlier, List<string> courseOrder, List<string> errors)
    {
        string where = $"{module.Slug}/module.yml";
        foreach (string name in names ?? [])
        {
            var match = earlier.FirstOrDefault(m => string.Equals(m.Slug, name, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                if (!module.Prerequisites.Contains(match))
                    module.Prerequisites.Add(match);
            }
            else if (string.Equals(name, module.Slug, StringComparison.OrdinalIgnoreCase))
                errors.Add($"{where}: a module can't be its own prerequisite.");
            else if (courseOrder.Contains(name, StringComparer.OrdinalIgnoreCase))
                errors.Add($"{where}: prerequisite '{name}' comes later in the course; list only modules that come before this one.");
            else
                errors.Add($"{where}: prerequisite '{name}' isn't a module in course.yml.");
        }
    }

    /// <summary>
    /// The module outcomes a unit checks: its questions' outcomes for a knowledge check, its own list for an exercise.
    /// Lessons name none, because the module's outcomes already say what its units teach.
    /// </summary>
    private static IReadOnlyList<Outcome> ReadAssesses(UnitType type, UnitFile meta, Quiz? quiz, IReadOnlyList<Outcome> outcomes, List<string> errors, string name)
    {
        if (meta.Outcomes is { Count: > 0 } && type != UnitType.Exercise)
        {
            errors.Add(type == UnitType.Quiz
                ? $"{name}: a knowledge check names an outcome on each question (outcome:), not in its front matter."
                : $"{name}: lessons don't name outcomes; the module's outcomes say what its units teach.");
            return [];
        }
        if (type == UnitType.Quiz)
            return quiz?.Questions.Select(q => q.Outcome).OfType<Outcome>().Distinct().ToList() ?? [];

        var assesses = new List<Outcome>();
        foreach (string id in meta.Outcomes ?? [])
        {
            var outcome = outcomes.FirstOrDefault(o => o.Id == id);
            if (outcome == null)
                errors.Add($"{name}: outcome '{id}' isn't one of this module's outcomes.");
            else if (!assesses.Contains(outcome))
                assesses.Add(outcome);
        }
        return assesses;
    }

    /// <summary>The unit's review date, if it has one: a real date no later than <paramref name="latest"/>.</summary>
    private static DateOnly? ReadReviewDate(string? value, DateOnly latest, List<string> errors, string name)
    {
        if (value == null)
            return null;
        if (!DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            errors.Add($"{name}: reviewed must be a date like 2026-11-10 (got '{value}').");
            return null;
        }
        if (date > latest)
        {
            errors.Add($"{name}: reviewed is {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}, which is in the future.");
            return null;
        }
        return date;
    }

    private static string BuildClientJson(List<LearnPath> paths, List<LearnModule> modules, List<LearnUnit> units, CourseFile config)
    {
        var client = new
        {
            units = units.Select(u => new { id = u.Id, module = u.Module.Slug, type = u.Type.ToString().ToLowerInvariant(), xp = u.Xp, title = u.Title, url = u.Url }),
            modules = modules.Select(m => new
            {
                slug = m.Slug, title = m.Title, path = m.Path.Slug, url = m.Url,
                units = m.Units.Select(u => u.Id), bonus = config.Xp.Module,
                badge = new { name = m.Badge.Name, description = m.Badge.Description },
            }),
            paths = paths.Select(p => new
            {
                slug = p.Slug, title = p.Title, difficulty = p.Difficulty, modules = p.Modules.Select(m => m.Slug), bonus = config.Xp.Path,
                trophy = new { name = p.Trophy.Name, description = p.Trophy.Description },
            }),
            levels = config.Levels.OrderBy(l => l.Xp).Select(l => new { level = l.Level, title = l.Title, xp = l.Xp }),
            achievements = config.Achievements.Select(a => new { id = a.Id, name = a.Name, description = a.Description }),
            quiz = new { perfectBonus = config.Xp.PerfectQuiz },
        };
        // The default encoder escapes < and >, so this is safe inside a <script type="application/json">.
        return JsonSerializer.Serialize(client, Json);
    }

    /// <summary>Markdown for a single line (quiz prompts and choices), without the wrapping paragraph.</summary>
    private static string InlineHtml(string markdown)
    {
        string html = Markdown.ToHtml(markdown ?? "", Pipeline).Trim();
        var single = SingleParagraphPattern().Match(html);
        return single.Success ? single.Groups[1].Value : html;
    }

    private static string PlainText(ContainerInline? inline)
    {
        var text = new StringBuilder();
        foreach (var child in inline?.Descendants() ?? [])
        {
            if (child is LiteralInline literal)
                text.Append(literal.Content);
            else if (child is CodeInline code)
                text.Append(code.Content);
        }
        return text.ToString();
    }

    private static JsonObject ReadYaml(string file)
    {
        if (!File.Exists(file))
            throw new InvalidDataException($"{file} is missing.");
        return ParseYaml(File.ReadAllText(file));
    }

    private static JsonObject ParseYaml(string yaml)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));
        if (stream.Documents.Count == 0)
            return new JsonObject();
        return ToJson(stream.Documents[0].RootNode) as JsonObject
            ?? throw new InvalidDataException("expected key: value pairs at the top level.");
    }

    /// <summary>
    /// YAML to JSON with real types: unquoted true/false and numbers become booleans and numbers,
    /// quoted and block scalars stay strings (so starter code and labels are never reinterpreted).
    /// </summary>
    private static JsonNode? ToJson(YamlNode node) => node switch
    {
        YamlMappingNode map => new JsonObject(map.Children.Select(kv =>
            KeyValuePair.Create(((YamlScalarNode)kv.Key).Value ?? "", ToJson(kv.Value)))),
        YamlSequenceNode list => new JsonArray(list.Children.Select(ToJson).ToArray()),
        YamlScalarNode scalar => Scalar(scalar),
        _ => null,
    };

    private static JsonNode? Scalar(YamlScalarNode scalar)
    {
        string? value = scalar.Value;
        if (scalar.Style == ScalarStyle.Plain)
        {
            if (value is null or "" or "~" or "null")
                return null;
            if (value is "true" or "false")
                return JsonValue.Create(value == "true");
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long whole))
                return JsonValue.Create(whole);
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                return JsonValue.Create(number);
        }
        return JsonValue.Create(value);
    }

    [GeneratedRegex(@"^\d+[-_]")]
    private static partial Regex UnitSlugPattern();

    [GeneratedRegex(@"^<p>(.*)</p>$", RegexOptions.Singleline)]
    private static partial Regex SingleParagraphPattern();

    /// <summary>Outcome ids are short lowercase slugs, such as write-loop.</summary>
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex OutcomeIdPattern();

    private sealed record CourseFile(List<LevelRule> Levels, XpRules Xp, List<Achievement> Achievements, List<PathFile> Paths);
    private sealed record PathFile(string Slug, string Title, string Difficulty, string Summary, Award Trophy, List<string> Modules,
        string? Entry = null, List<OutcomeFile?>? Outcomes = null);
    private sealed record ModuleFile(string Title, string Summary, string Icon, Award Badge,
        List<OutcomeFile?>? Outcomes = null, List<string>? Topics = null, List<string>? Prerequisites = null);
    /// <summary>A module or path outcome; only a path's outcomes list the <paramref name="Modules"/> that build them.</summary>
    private sealed record OutcomeFile(string? Id, string? Bloom, string? Text, List<string>? Modules = null);
    private sealed record UnitFile(string Title, string? Summary, string Type, int? Minutes, int? Xp, int? Pass, List<QuestionFile>? Questions, JsonObject? Exercise,
        List<string>? Outcomes = null, string? Reviewed = null);
    private sealed record QuestionFile(string Prompt, List<string>? Choices, int Answer, string? Explanation, string? Outcome = null);
}
