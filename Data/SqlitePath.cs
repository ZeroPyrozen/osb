#nullable enable

using Microsoft.Data.Sqlite;

namespace osb.Data;

public static class SqlitePath
{
    /// <summary>
    /// Makes a relative Data Source absolute, against <paramref name="contentRoot"/>, and creates
    /// its folder. In-memory databases and absolute paths are kept as they are.
    /// </summary>
    public static string Resolve(string connectionString, string contentRoot)
    {
        var csb = new SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrEmpty(csb.DataSource) && csb.DataSource != ":memory:" && !Path.IsPathRooted(csb.DataSource))
            csb.DataSource = Path.GetFullPath(Path.Combine(contentRoot, csb.DataSource));
        if (Path.GetDirectoryName(csb.DataSource) is { Length: > 0 } folder)
            Directory.CreateDirectory(folder);
        return csb.ToString();
    }
}
