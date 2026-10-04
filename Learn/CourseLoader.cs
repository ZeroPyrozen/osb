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
///   <item><c>course.yml</c>: XP rules, levels, achievements and the paths with their module order.</item>
///   <item><c>&lt;module&gt;/module.yml</c>: a module's title, summary, icon and badge.</item>
///   <item><c>&lt;module&gt;/NN-&lt;unit&gt;.md</c>: units in file-name order, Markdown with YAML front matter.</item>
/// </list>
/// Everything is validated up front; problems are reported together, with file names.
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

    public static Course Load(string root)
    {
        var errors = new List<string>();
        string Rel(string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

        var config = ReadYaml(Path.Combine(root, "course.yml")).Deserialize<CourseFile>(Json)
            ?? throw new InvalidDataException("course.yml is empty.");

        var paths = new List<LearnPath>();
        var modules = new List<LearnModule>();
        var units = new List<LearnUnit>();
        var seenModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pathFile in config.Paths)
        {
            var path = new LearnPath
            {
                Slug = pathFile.Slug,
                Title = pathFile.Title,
                Level = pathFile.Level,
                Summary = pathFile.Summary,
                Trophy = pathFile.Trophy,
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

                var moduleFile = ReadYaml(moduleYml).Deserialize<ModuleFile>(Json)!;
                if (!Icons.Exists(moduleFile.Icon))
                    errors.Add($"{slug}/module.yml: unknown icon '{moduleFile.Icon}' (see Helpers/Icons.cs).");

                var module = new LearnModule
                {
                    Slug = slug,
                    Title = moduleFile.Title,
                    Summary = moduleFile.Summary,
                    Icon = moduleFile.Icon,
                    Badge = moduleFile.Badge,
                    Path = path,
                    Number = modules.Count + 1,
                };
                path.Modules.Add(module);
                modules.Add(module);

                foreach (string file in Directory.GetFiles(folder, "*.md").OrderBy(f => f, StringComparer.Ordinal))
                {
                    try
                    {
                        var unit = ReadUnit(file, module, config.Xp, errors, Rel(file));
                        if (unit == null)
                            continue;
                        unit.Number = module.Units.Count + 1;
                        module.Units.Add(unit);
                        units.Add(unit);
                    }
                    catch (Exception e) when (e is YamlException or JsonException or InvalidDataException)
                    {
                        errors.Add($"{Rel(file)}: {e.Message}");
                    }
                }
                if (module.Units.Count == 0)
                    errors.Add($"{slug}: the module has no units (NN-name.md files).");
            }
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
        };
    }

    private static LearnUnit? ReadUnit(string file, LearnModule module, XpRules xp, List<string> errors, string name)
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
        Quiz? quiz = type == UnitType.Quiz ? ReadQuiz(meta, errors, name) : null;
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
        };
    }

    private static Quiz? ReadQuiz(UnitFile meta, List<string> errors, string name)
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
            questions.Add(new QuizQuestion(
                InlineHtml(q.Prompt),
                (q.Choices ?? new()).Select(InlineHtml).ToList(),
                q.Answer,
                q.Explanation != null ? Markdown.ToHtml(q.Explanation, Pipeline) : ""));
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
                slug = p.Slug, title = p.Title, level = p.Level, modules = p.Modules.Select(m => m.Slug), bonus = config.Xp.Path,
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

    private sealed record CourseFile(List<LevelRule> Levels, XpRules Xp, List<Achievement> Achievements, List<PathFile> Paths);
    private sealed record PathFile(string Slug, string Title, string Level, string Summary, Award Trophy, List<string> Modules);
    private sealed record ModuleFile(string Title, string Summary, string Icon, Award Badge);
    private sealed record UnitFile(string Title, string? Summary, string Type, int? Minutes, int? Xp, int? Pass, List<QuestionFile>? Questions, JsonObject? Exercise);
    private sealed record QuestionFile(string Prompt, List<string>? Choices, int Answer, string? Explanation);
}
