using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using osb.Helpers;

namespace osb.Tests.Infrastructure;

/// <summary>What a person does in the browser, for the web tests.</summary>
public static partial class Browser
{
    /// <summary>
    /// Logs in through the real /auth flow, with the fake osu! answering as this account.
    /// Returns where the site sent the browser afterwards.
    /// </summary>
    public static async Task<string> LoginAsync(this HttpClient client, OsbWebFactory site, int userId, string username, string returnUrl = "/")
    {
        site.Osu.Accounts[userId] = new WebUserModel { ID = userId, Username = username, AvatarURL = $"https://a.ppy.sh/{userId}" };

        var start = await client.GetAsync("/auth?returnUrl=" + Uri.EscapeDataString(returnUrl));
        Assert.Equal(HttpStatusCode.Redirect, start.StatusCode);
        string state = StateFrom(start);

        var back = await client.GetAsync($"/auth/authorized?code=user-{userId}&state={Uri.EscapeDataString(state)}");
        Assert.Equal(HttpStatusCode.Redirect, back.StatusCode);
        return back.Headers.Location!.OriginalString;
    }

    /// <summary>The OAuth state from a redirect to the fake osu! login page.</summary>
    public static string StateFrom(HttpResponseMessage redirectToOsu) =>
        QueryHelpers.ParseQuery(redirectToOsu.Headers.Location!.Query)["state"].ToString();

    /// <summary>The CSRF token learn pages give logged-in learners (the csrf-token meta tag).</summary>
    public static async Task<string> GetCsrfTokenAsync(this HttpClient client)
    {
        string html = await client.GetStringAsync("/learn");
        var match = CsrfMeta().Match(html);
        Assert.True(match.Success, "The learn page has no csrf-token meta tag. Is the client logged in?");
        return match.Groups[1].Value;
    }

    /// <summary>The antiforgery token of the first form on a page, such as the nav's logout form.</summary>
    public static async Task<string> GetFormTokenAsync(this HttpClient client, string path)
    {
        string html = await client.GetStringAsync(path);
        var match = FormToken().Match(html);
        Assert.True(match.Success, $"{path} has no form with an antiforgery token.");
        return match.Groups[1].Value;
    }

    /// <summary>The cookies a response sets, by name (the raw Set-Cookie value).</summary>
    public static Dictionary<string, string> SetCookies(this HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.ToDictionary(v => v[..v.IndexOf('=')], v => v, StringComparer.Ordinal)
            : new Dictionary<string, string>();

    [GeneratedRegex("<meta name=\"csrf-token\" content=\"([^\"]+)\"")]
    private static partial Regex CsrfMeta();

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex FormToken();
}
