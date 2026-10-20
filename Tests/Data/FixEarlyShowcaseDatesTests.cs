using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using osb.Data;

namespace osb.Tests.Data;

/// <summary>The FixEarlyShowcaseDates migration, run on a database that still has the old dates.</summary>
public sealed class FixEarlyShowcaseDatesTests : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public FixEarlyShowcaseDatesTests() => connection.Open();

    public void Dispose() => connection.Dispose();

    private OsbDbContext Context() => new(new DbContextOptionsBuilder<OsbDbContext>().UseSqlite(connection).Options);

    private static Beatmapset Set(int id, OsuUser host, DateOnly submitted, DateOnly showcased, bool changedOnSite = false) => new()
    {
        Id = id, Title = "Song", Artist = "Artist", Host = host, Medium = "Storybrew",
        SubmittedOn = submitted, ShowcasedOn = showcased, ChangedOnSiteAt = changedOnSite ? DateTime.UtcNow : null,
    };

    [Fact]
    public async Task AStoryboardShowcasedBeforeItsSubmission_GetsTheSubmissionDate_UnlessItWasChangedOnTheSite()
    {
        await using (var db = Context())
        {
            await db.GetService<IMigrator>().MigrateAsync("20261007080434_ShowcaseSubmissions");
            var host = new OsuUser { Id = 1, Username = "Host" };
            db.Beatmapsets.AddRange(
                Set(550344, host, submitted: new(2016, 12, 25), showcased: new(2013, 12, 25)),
                Set(2, host, submitted: new(2020, 1, 1), showcased: new(2020, 2, 1)),
                Set(3, host, submitted: new(2020, 3, 1), showcased: new(2020, 2, 1), changedOnSite: true));
            await db.SaveChangesAsync();
        }

        await using (var db = Context())
            await db.Database.MigrateAsync();

        await using var check = Context();
        var showcased = await check.Beatmapsets.ToDictionaryAsync(s => s.Id, s => s.ShowcasedOn);
        Assert.Equal(new DateOnly(2016, 12, 25), showcased[550344]);
        Assert.Equal(new DateOnly(2020, 2, 1), showcased[2]);
        Assert.Equal(new DateOnly(2020, 2, 1), showcased[3]);
    }
}
