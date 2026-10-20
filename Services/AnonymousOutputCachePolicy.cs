#nullable enable

using Microsoft.AspNetCore.OutputCaching;

namespace osb.Services;

/// <summary>
/// Caches the read-heavy content pages for visitors who aren't logged in. Anonymous pages are identical
/// for everyone and set no cookies, so they're safe to share; a logged-in request (one carrying the
/// <c>osb.auth</c> cookie) always skips the cache and is rendered fresh, because its nav is personalised.
///
/// Cached responses also carry <c>Cache-Control: public</c> so a CDN in front of the site (Cloudflare)
/// can edge-cache them too. A short TTL keeps approvals and edits appearing within about a minute without
/// any explicit cache invalidation.
///
/// Caching is on by default and can be turned off with <c>OutputCache:Enabled=false</c> (the test suite
/// does this so unrelated tests aren't served stale pages).
/// </summary>
public sealed class AnonymousOutputCachePolicy : IOutputCachePolicy
{
    private const string AuthCookie = "osb.auth";
    private const int MaxAgeSeconds = 60;

    private readonly bool enabled;

    public AnonymousOutputCachePolicy(IConfiguration configuration) =>
        enabled = configuration.GetValue("OutputCache:Enabled", true);

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var http = context.HttpContext;
        bool cacheable = enabled
            && HttpMethods.IsGet(http.Request.Method)
            && !http.Request.Cookies.ContainsKey(AuthCookie);

        context.EnableOutputCaching = cacheable;
        context.AllowCacheLookup = cacheable;
        context.AllowCacheStorage = cacheable;
        context.AllowLocking = true;
        // Different search, tag, tool and page combinations are different pages.
        context.CacheVaryByRules.QueryKeys = "*";

        if (cacheable)
        {
            context.ResponseExpirationTimeSpan = TimeSpan.FromSeconds(MaxAgeSeconds);
            // Add the CDN cache header just before the response is sent. It can't be set in
            // ServeResponseAsync, which runs once the body has already started streaming.
            http.Response.OnStarting(static state =>
            {
                var response = (HttpResponse)state;
                if (response.StatusCode == StatusCodes.Status200OK && !response.Headers.ContainsKey("Set-Cookie"))
                    response.Headers.CacheControl = $"public, max-age={MaxAgeSeconds}";
                return Task.CompletedTask;
            }, http.Response);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        // Never store an error response, or one that sets a cookie (a personalised page).
        var response = context.HttpContext.Response;
        if (response.StatusCode != StatusCodes.Status200OK || response.Headers.ContainsKey("Set-Cookie"))
            context.AllowCacheStorage = false;
        return ValueTask.CompletedTask;
    }
}
