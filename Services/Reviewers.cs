#nullable enable

using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace osb.Services;

/// <summary>
/// The osb team members who review showcase submissions: the osu! user IDs in the Showcase:Reviewers
/// setting, separated by commas. On the Pi that's <c>Showcase__Reviewers</c> in /etc/osb/osb.env. It's read
/// at startup, so a change applies after a restart; reviewers don't need to log in again.
/// </summary>
public sealed class ReviewerList
{
    public const string Role = "Reviewer";

    private readonly HashSet<int> ids = new();

    public ReviewerList(IConfiguration configuration, ILogger<ReviewerList> logger)
    {
        string setting = configuration["Showcase:Reviewers"] ?? "";
        foreach (string entry in setting.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(entry, NumberStyles.None, CultureInfo.InvariantCulture, out int id) && id > 0)
                ids.Add(id);
            else
                logger.LogWarning("Showcase:Reviewers lists '{Entry}', which isn't an osu! user ID.", entry);
        }
    }

    public bool Contains(int userId) => ids.Contains(userId);
}

/// <summary>Gives reviewers the Reviewer role on every request, from their osu! user ID.</summary>
public sealed class ReviewerClaims(ReviewerList reviewers) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(ReviewerList.Role)
            || !int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out int id)
            || !reviewers.Contains(id))
            return Task.FromResult(principal);

        // A copy, so the principal the cookie handler cached is left as it was.
        var reviewer = principal.Clone();
        reviewer.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Role, ReviewerList.Role)]));
        return Task.FromResult(reviewer);
    }
}
