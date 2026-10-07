using osb.Learn;

namespace osb.Tests.Infrastructure;

/// <summary>
/// A small learn course written to a temporary folder, laid out like the real one
/// (<c>&lt;content root&gt;/Content/Learn</c>), for testing the course loader and provider.
/// </summary>
public sealed class CourseFolder : IDisposable
{
    public string ContentRoot { get; } = Directory.CreateTempSubdirectory("osb-course-").FullName;

    /// <summary>The course folder itself, with course.yml.</summary>
    public string Root => Path.Combine(ContentRoot, "Content", "Learn");

    /// <summary>A course with one path listing <paramref name="modules"/>, in that order.</summary>
    public static CourseFolder WithModules(params string[] modules) => new CourseFolder().Course(modules);

    /// <summary>Writes course.yml with the usual XP rules and levels, and one path with these modules.</summary>
    public CourseFolder Course(params string[] modules) => File("course.yml", $$"""
        xp: { lesson: 10, quiz: 20, exercise: 30, perfectQuiz: 5, module: 50, path: 150 }
        levels:
          - { level: 2, title: Sprite Wrangler, xp: 60 }
          - { level: 1, title: Spectator, xp: 0 }
        achievements:
          - { id: first-steps, name: First steps, description: Complete your first unit. }
        paths:
          - slug: beginner
            title: Your first storyboard
            level: Beginner
            summary: The basics.
            trophy: { name: First Light, description: Finish the Beginner path. }
            modules: [{{string.Join(", ", modules)}}]
        """);

    public CourseFolder Module(string slug, string icon = "book") => File($"{slug}/module.yml", $$"""
        title: Module {{slug}}
        summary: All about {{slug}}.
        icon: {{icon}}
        badge: { name: Badge {{slug}}, description: Finished {{slug}}. }
        """);

    public CourseFolder Lesson(string module, string file, string title = "A lesson", string body = "Some text.") =>
        File($"{module}/{file}", $"""
            ---
            title: {title}
            type: lesson
            ---

            {body}
            """);

    /// <summary>A unit file with its YAML front matter (without the --- lines) and a body.</summary>
    public CourseFolder Unit(string module, string file, string frontMatter, string body = "Some text.") =>
        File($"{module}/{file}", $"---\n{frontMatter.Trim()}\n---\n\n{body}\n");

    public CourseFolder File(string path, string text)
    {
        string full = Path.Combine(Root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        System.IO.File.WriteAllText(full, text);
        return this;
    }

    public Course Load() => CourseLoader.Load(Root);

    /// <summary>Loads the course, expecting it to fail, and returns the reported problems.</summary>
    public string Problems() => Assert.Throws<InvalidDataException>(() => Load()).Message;

    public void Dispose()
    {
        try
        {
            Directory.Delete(ContentRoot, recursive: true);
        }
        catch (IOException)
        {
            // A file watcher may still hold the folder for a moment; the OS cleans temp folders anyway.
        }
    }
}
