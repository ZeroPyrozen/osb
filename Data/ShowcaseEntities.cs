#nullable enable

namespace osb.Data;

/// <summary>
/// An osu! account known to the site: a member of the osb community, a storyboarder or a beatmapset host.
/// The key is the osu! user ID.
/// </summary>
public class OsuUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";

    /// <summary>Listed on the Community page (a member of the osb Discord server).</summary>
    public bool IsCommunityMember { get; set; }

    public List<CommunityRole> Roles { get; set; } = new();
    public List<BeatmapsetStoryboarder> Storyboards { get; set; } = new();
    public List<Beatmapset> HostedBeatmapsets { get; set; } = new();

    public string AvatarUrl => $"https://a.ppy.sh/{Id}";
    public string OsuProfileUrl => $"https://osu.ppy.sh/users/{Id}";

    /// <summary>The highest-ranked role, shown next to the username.</summary>
    public CommunityRole? PrimaryRole => Roles.OrderByDescending(r => r.Rank).FirstOrDefault();
}

/// <summary>A role from the osb Discord server, such as Mentor or Storyboarder.</summary>
public class CommunityRole
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>The role colour as a CSS hex value, e.g. <c>#3498db</c>.</summary>
    public string Colour { get; set; } = "";

    /// <summary>Higher ranks are listed first and decide a member's primary role.</summary>
    public int Rank { get; set; }

    public List<OsuUser> Members { get; set; } = new();
}

/// <summary>A showcase tag such as Particles or Lyrics. Tag ratings add up to a storyboard's OSB level.</summary>
public class StoryboardTag
{
    public int Id { get; set; }

    /// <summary>URL-friendly name, also the fragment ID of the tag's icon in the showcase icon sprite.</summary>
    public string Slug { get; set; } = "";

    public string Name { get; set; } = "";
    public int Rating { get; set; }

    public List<Beatmapset> Beatmapsets { get; set; } = new();
}

/// <summary>A storyboarded beatmapset featured in the showcase. The key is the osu! beatmapset ID.</summary>
public class Beatmapset
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";

    public int HostId { get; set; }
    public OsuUser Host { get; set; } = null!;

    /// <summary>The tool or language the storyboard was made with, e.g. Storybrew or SGL.</summary>
    public string Medium { get; set; } = "";

    public DateOnly SubmittedOn { get; set; }
    public DateOnly ShowcasedOn { get; set; }

    /// <summary>A YouTube embed URL of the storyboard, if there is one.</summary>
    public string? VideoUrl { get; set; }

    public List<BeatmapsetStoryboarder> Storyboarders { get; set; } = new();
    public List<StoryboardTag> Tags { get; set; } = new();

    public string CoverUrl => $"https://assets.ppy.sh/beatmaps/{Id}/covers/cover.jpg";
    public string CardUrl => $"https://assets.ppy.sh/beatmaps/{Id}/covers/card.jpg";
    public string ListUrl => $"https://assets.ppy.sh/beatmaps/{Id}/covers/list.jpg";
    public string OsuPageUrl => $"https://osu.ppy.sh/beatmapsets/{Id}";

    /// <summary>The "OSB level": the sum of the storyboard's tag ratings.</summary>
    public int OsbLevel => Tags.Sum(t => t.Rating);

    /// <summary>Storyboarders in credit order.</summary>
    public IEnumerable<OsuUser> Credits => Storyboarders.OrderBy(s => s.Position).Select(s => s.User);
}

/// <summary>Credits a storyboarder on a beatmapset. <see cref="Position"/> keeps the credit order.</summary>
public class BeatmapsetStoryboarder
{
    public int BeatmapsetId { get; set; }
    public Beatmapset Beatmapset { get; set; } = null!;

    public int OsuUserId { get; set; }
    public OsuUser User { get; set; } = null!;

    public int Position { get; set; }
}
