#nullable enable

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using osb.Helpers;

namespace osb.Controllers;

/// <summary>
/// Logs people in with their osu! account (OAuth authorization code flow) and keeps them signed in
/// with a 30-day cookie. Only the "identify" scope is requested and no osu! token is stored.
/// </summary>
public class AuthController(IOsuWebHelper osu, ILogger<AuthController> logger) : Controller
{
    /// <summary>Holds the OAuth state and the page to return to while the user is on osu!.</summary>
    private const string StateCookie = "osb.oauth";

    public const string AvatarClaim = "osb:avatar";

    [HttpGet("/auth")]
    public IActionResult Index(string? returnUrl)
    {
        string state = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        string target = Url.IsLocalUrl(returnUrl) ? returnUrl! : "/";

        Response.Cookies.Append(StateCookie, state + "|" + target, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax, // sent on the top-level redirect back from osu!
            MaxAge = TimeSpan.FromMinutes(10),
            IsEssential = true,
        });

        return Redirect(osu.GetAuthorizationUrl(state));
    }

    [HttpGet("/auth/authorized")]
    public async Task<IActionResult> Authorized(string? code, string? state, string? error)
    {
        string? saved = Request.Cookies[StateCookie];
        Response.Cookies.Delete(StateCookie);

        string[]? parts = saved?.Split('|', 2);
        if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code) || parts is not { Length: 2 } || !SameState(parts[0], state))
        {
            logger.LogInformation("osu! login was cancelled or its state didn't match.");
            return Redirect("/");
        }

        var token = await osu.GenerateAccessTokenAuthCode(code);
        var user = token?.AccessToken != null ? await osu.GetOwnData(token.AccessToken) : null;
        if (user == null)
        {
            logger.LogWarning("osu! login failed: couldn't exchange the code or load the user.");
            return Redirect("/");
        }

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? $"User {user.ID}"),
            new Claim(AvatarClaim, user.AvatarURL ?? $"https://a.ppy.sh/{user.ID}"),
        ], CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        return LocalRedirect(Url.IsLocalUrl(parts[1]) ? parts[1] : "/");
    }

    [HttpPost("/auth/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    private static bool SameState(string expected, string? actual) =>
        actual != null && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(actual));
}
