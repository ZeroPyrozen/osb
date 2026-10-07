using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using osb.Data;

namespace osb.Tests.Infrastructure;

/// <summary>
/// An in-memory SQLite database with the site's real schema: every migration is applied, but
/// nothing is seeded. It lives as long as this object, and every context from
/// <see cref="CreateContext"/> shares it.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public TestDatabase()
    {
        connection.Open();
        using var db = CreateContext();
        db.Database.Migrate();
    }

    public OsbDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<OsbDbContext>().UseSqlite(connection).Options);

    public void Dispose() => connection.Dispose();
}
