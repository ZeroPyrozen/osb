#nullable enable

using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;

namespace osb.Services;

/// <summary>Someone named in a review form, as an osu! account. <see cref="FromOsu"/>: just looked up there.</summary>
internal sealed record Person(int Id, string Username, bool FromOsu = false);

/// <summary>
/// Turns the usernames, IDs and profile links reviewers type into osu! accounts. People the submission
/// already names, or the site already knows, need no lookup, so a review works while osu! is down as
/// long as no names changed.
/// </summary>
internal sealed class PeopleResolver(OsbDbContext db, IOsuWebHelper osu)
{
    /// <summary>
    /// The people in a comma-separated list, each once and in order, or a message saying who couldn't
    /// be found (or that osu! couldn't be asked).
    /// </summary>
    public async Task<(IReadOnlyList<Person> People, string? Error)> ResolveAsync(string? list, IReadOnlyList<Person> known, CancellationToken ct)
    {
        var people = new List<Person>();
        var unknown = new List<string>();
        foreach (string name in OsuLinks.ParseUsers(list))
        {
            var (person, unavailable) = await FindAsync(name, known, ct);
            if (unavailable)
                return ([], $"Couldn't reach osu! to look up {name}. Try again in a few minutes.");
            if (person == null)
                unknown.Add(name);
            else if (people.All(p => p.Id != person.Id))
                people.Add(person);
        }
        return unknown.Count > 0 ? ([], $"osu! has no player called {Format.List(unknown)}.") : (people, null);
    }

    /// <summary>
    /// The site's accounts for these people: new ones are added (not as community members), and
    /// people just looked up on osu! get their current username.
    /// </summary>
    public async Task<Dictionary<int, OsuUser>> SaveAsync(IEnumerable<Person> people, CancellationToken ct)
    {
        var everyone = people.GroupBy(p => p.Id).Select(g => g.OrderByDescending(p => p.FromOsu).First()).ToList();
        var ids = everyone.Select(p => p.Id).ToList();
        var users = await db.Users.Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id, ct);
        foreach (var person in everyone)
        {
            if (!users.TryGetValue(person.Id, out var user))
                db.Users.Add(users[person.Id] = new OsuUser { Id = person.Id, Username = person.Username });
            else if (person.FromOsu && user.Username != person.Username)
                user.Username = person.Username;
        }
        return users;
    }

    private async Task<(Person? Person, bool Unavailable)> FindAsync(string name, IReadOnlyList<Person> known, CancellationToken ct)
    {
        if (known.FirstOrDefault(k => string.Equals(k.Username, name, StringComparison.OrdinalIgnoreCase)) is { } named)
            return (named, false);

        bool isId = int.TryParse(name, out int id) && name.All(char.IsAsciiDigit);
        if (isId)
        {
            if (known.FirstOrDefault(k => k.Id == id) is { } numbered)
                return (numbered, false);
            if (await db.Users.FindAsync([id], ct) is { } user)
                return (new Person(user.Id, user.Username), false);
        }
        else
        {
            string lower = name.ToLowerInvariant();
            var sameName = await db.Users.Where(u => u.Username.ToLower() == lower).Take(2).ToListAsync(ct);
            if (sameName.Count == 1)
                return (new Person(sameName[0].Id, sameName[0].Username), false);
        }

        var found = await osu.FindUser(name);
        if (found.Unavailable)
            return (null, true);
        return (found.Value == null ? null : new Person(found.Value.ID, found.Value.Username, FromOsu: true), false);
    }
}
