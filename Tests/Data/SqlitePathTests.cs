using Microsoft.Data.Sqlite;
using osb.Data;

namespace osb.Tests.Data;

public sealed class SqlitePathTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("osb-sqlite-").FullName;

    public void Dispose() => Directory.Delete(root, recursive: true);

    private static string DataSource(string connectionString) => new SqliteConnectionStringBuilder(connectionString).DataSource;

    [Fact]
    public void RelativePaths_ResolveAgainstTheContentRoot_AndTheirFolderIsCreated()
    {
        string resolved = SqlitePath.Resolve("Data Source=App_Data/osb.db", root);

        Assert.Equal(Path.Combine(root, "App_Data", "osb.db"), DataSource(resolved));
        Assert.True(Directory.Exists(Path.Combine(root, "App_Data")));
    }

    [Fact]
    public void AbsolutePaths_StayAsTheyAre()
    {
        string file = Path.Combine(root, "data", "site.db");

        string resolved = SqlitePath.Resolve($"Data Source={file}", Path.Combine(root, "elsewhere"));

        Assert.Equal(file, DataSource(resolved));
        Assert.True(Directory.Exists(Path.Combine(root, "data")));
    }

    [Fact]
    public void InMemoryDatabases_StayInMemory() =>
        Assert.Equal(":memory:", DataSource(SqlitePath.Resolve("Data Source=:memory:", root)));

    [Fact]
    public void OtherSettings_AreKept()
    {
        var resolved = new SqliteConnectionStringBuilder(SqlitePath.Resolve("Data Source=osb.db;Cache=Shared", root));

        Assert.Equal(SqliteCacheMode.Shared, resolved.Cache);
    }
}
