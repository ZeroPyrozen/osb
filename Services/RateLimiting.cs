#nullable enable

using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace osb.Services;

/// <summary>
/// Request rate limits for the public endpoints that cost the most to abuse: osu! logins, showcase
/// submissions, the progress API and the osu!-backed profile lookups. Each caller is limited per
/// authenticated osu! user where there is one, otherwise per client IP (the real client address,
/// thanks to the forwarded-headers config). Limits read from the <c>RateLimits</c> configuration
/// section, so they can be tuned — or raised out of the way in tests — without a rebuild.
/// </summary>
public static class RateLimiting
{
    public const string Auth = "auth";
    public const string Submit = "submit";
    public const string Progress = "progress";
    public const string OsuLookup = "osu-lookup";

    public static IServiceCollection AddOsbRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection("RateLimits");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(Auth, ByIp(Auth, limits.GetValue("AuthPerMinute", 20)));
            options.AddPolicy(Submit, ByUserOrIp(Submit, limits.GetValue("SubmitPerMinute", 10)));
            options.AddPolicy(Progress, ByUserOrIp(Progress, limits.GetValue("ProgressPerMinute", 60)));
            options.AddPolicy(OsuLookup, ByIp(OsuLookup, limits.GetValue("OsuLookupPerMinute", 40)));
        });
        return services;
    }

    private static Func<HttpContext, RateLimitPartition<string>> ByUserOrIp(string policy, int perMinute) =>
        context => Window($"{policy}:{ClientKey(context)}", perMinute);

    private static Func<HttpContext, RateLimitPartition<string>> ByIp(string policy, int perMinute) =>
        context => Window($"{policy}:ip:{Ip(context)}", perMinute);

    private static RateLimitPartition<string> Window(string key, int perMinute) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = Math.Max(1, perMinute),
            Window = TimeSpan.FromMinutes(1),
        });

    private static string ClientKey(HttpContext context) =>
        context.User.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } id ? $"u:{id}" : $"ip:{Ip(context)}";

    private static string Ip(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
