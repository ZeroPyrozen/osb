#nullable enable

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace osb.Learn;

/// <summary>
/// Holds the loaded course. Loading happens at startup, so broken content stops the app (and a deploy's
/// health check rolls it back). In Development the course reloads when a file under Content/Learn
/// changes; if the edited content has problems, the last good version stays and the problems are logged.
/// </summary>
public sealed class CourseProvider : IDisposable
{
    private readonly string root;
    private readonly ILogger<CourseProvider> logger;
    private readonly object gate = new();
    // Held in a field on purpose: a file watcher that nothing references gets garbage collected and
    // silently stops reporting changes.
    private readonly PhysicalFileProvider? files;
    private readonly IDisposable? watch;
    private Course course;
    private volatile bool stale;

    public CourseProvider(IWebHostEnvironment env, ILogger<CourseProvider> logger)
    {
        this.logger = logger;
        root = Path.Combine(env.ContentRootPath, "Content", "Learn");
        course = CourseLoader.Load(root);
        logger.LogInformation("Loaded the learn course: {Paths} paths, {Modules} modules, {Units} units.",
            course.Paths.Count, course.Modules.Count, course.Units.Count);

        if (env.IsDevelopment())
        {
            files = new PhysicalFileProvider(root);
            watch = ChangeToken.OnChange(() => files.Watch("**/*"), () => stale = true);
        }
    }

    public void Dispose()
    {
        watch?.Dispose();
        files?.Dispose();
    }

    public Course Current
    {
        get
        {
            if (!stale)
                return course;
            lock (gate)
            {
                if (stale)
                {
                    stale = false;
                    try
                    {
                        course = CourseLoader.Load(root);
                        logger.LogInformation("Reloaded the learn course after a content change.");
                    }
                    catch (Exception e) when (e is InvalidDataException or IOException or YamlDotNet.Core.YamlException or System.Text.Json.JsonException)
                    {
                        logger.LogError("Learn content has problems; still showing the last good version. {Problems}", e.Message);
                    }
                }
                return course;
            }
        }
    }
}
