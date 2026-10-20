#nullable enable

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace osb.Data;

/// <summary>
/// Sets the SQLite pragmas the site needs on every connection as it opens:
/// <list type="bullet">
///   <item><c>journal_mode=WAL</c> — readers don't block the writer, so browsing the showcase keeps
///   working while a submission or a progress sync writes. Persistent once set; re-applied cheaply.</item>
///   <item><c>busy_timeout=5000</c> — a writer that finds the database locked waits up to 5 seconds for
///   the lock instead of failing immediately with "database is locked". Per connection, so set here.</item>
///   <item><c>synchronous=NORMAL</c> — the safe, faster pairing with WAL (a crash can lose only the last
///   transaction, never corrupt the database). Worth it on the Pi's SD card.</item>
/// </list>
/// In-memory databases ignore WAL, which is harmless, so the tests share this configuration.
/// </summary>
public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    private const string Pragmas = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000; PRAGMA synchronous=NORMAL;";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = connection.CreateCommand();
        command.CommandText = Pragmas;
        command.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = Pragmas;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
