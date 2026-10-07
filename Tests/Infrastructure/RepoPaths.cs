namespace osb.Tests.Infrastructure;

/// <summary>Folders of the repository the tests were built from.</summary>
public static class RepoPaths
{
    /// <summary>The repo root: the folder with osb.csproj, found by walking up from the test binaries.</summary>
    public static string Root { get; } = FindRoot();

    public static string Learn => Path.Combine(Root, "Content", "Learn");

    public static string WebRoot => Path.Combine(Root, "wwwroot");

    private static string FindRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "osb.csproj")))
                return dir.FullName;
        }
        throw new InvalidOperationException($"Couldn't find osb.csproj in {AppContext.BaseDirectory} or above it.");
    }
}
