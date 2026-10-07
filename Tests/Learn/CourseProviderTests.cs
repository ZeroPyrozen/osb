using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using osb.Learn;
using osb.Tests.Infrastructure;

namespace osb.Tests.Learn;

/// <summary>Reloading uses real file watching, so these tests wait (briefly) for the OS to report changes.</summary>
public class CourseProviderTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(15);

    private static CourseFolder OneLesson(string title) =>
        CourseFolder.WithModules("basics").Module("basics").Lesson("basics", "01-intro.md", title);

    [Fact]
    public void Production_KeepsTheCourseFromStartup()
    {
        using var folder = OneLesson("Before");
        using var provider = new CourseProvider(new TestEnvironment(folder.ContentRoot, "Production"), NullLogger<CourseProvider>.Instance);
        var loaded = provider.Current;

        folder.Lesson("basics", "01-intro.md", "After");
        Thread.Sleep(TimeSpan.FromSeconds(1));

        Assert.Same(loaded, provider.Current);
    }

    /// <summary>
    /// The file watcher used to be referenced by nothing, so the garbage collector removed it and
    /// edits silently stopped reloading. The collection here makes that bug show up straight away.
    /// </summary>
    [Fact]
    public async Task Development_ReloadsAfterAnEdit_EvenAfterAGarbageCollection()
    {
        using var folder = OneLesson("Before");
        using var provider = new CourseProvider(new TestEnvironment(folder.ContentRoot, "Development"), NullLogger<CourseProvider>.Instance);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        folder.Lesson("basics", "01-intro.md", "After");

        Assert.True(await Eventually(() => provider.Current.Units[0].Title == "After"), "The course didn't reload after the edit.");
    }

    [Fact]
    public async Task Development_KeepsTheLastGoodCourse_WhenAnEditBreaksIt()
    {
        using var folder = OneLesson("Before");
        var log = new ListLogger();
        using var provider = new CourseProvider(new TestEnvironment(folder.ContentRoot, "Development"), log);
        var good = provider.Current;

        folder.Unit("basics", "01-intro.md", "title: Broken\ntype: video");
        Assert.True(await Eventually(() => provider.Current != null && log.Errors.Count > 0), "The broken edit wasn't noticed.");
        Assert.Same(good, provider.Current);
        Assert.Contains("type must be lesson, quiz or exercise", log.Errors[0]);

        folder.Lesson("basics", "01-intro.md", "Fixed");
        Assert.True(await Eventually(() => provider.Current.Units[0].Title == "Fixed"), "The fixed course didn't load.");
    }

    private static async Task<bool> Eventually(Func<bool> condition)
    {
        var until = DateTime.UtcNow + Patience;
        while (DateTime.UtcNow < until)
        {
            if (condition())
                return true;
            await Task.Delay(50);
        }
        return condition();
    }

    private sealed class ListLogger : ILogger<CourseProvider>
    {
        private readonly List<string> errors = new();

        public IReadOnlyList<string> Errors
        {
            get { lock (errors) return errors.ToList(); }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel >= LogLevel.Error)
                lock (errors) errors.Add(formatter(state, exception));
        }
    }
}
