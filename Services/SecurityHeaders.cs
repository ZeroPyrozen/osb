#nullable enable

namespace osb.Services;

/// <summary>
/// Adds a Content-Security-Policy and the standard hardening headers to every response. The external
/// surface is small and known, so the document policy is tight: scripts, styles, fonts and XHR are
/// same-origin, images come from osu! and YouTube's thumbnail host, and the only frame is the
/// click-to-load video player.
///
/// The one exception is the storyboard playground, which compiles a learner's code with
/// <c>new Function</c> inside a Web Worker — something a plain <c>script-src 'self'</c> blocks. Rather
/// than weaken the whole site, that one worker script is served with its own policy that allows eval,
/// so eval is confined to the sandbox and the rest of the site, lesson pages included, still forbids it.
/// </summary>
public static class SecurityHeaders
{
    /// <summary>The worker that runs learners' scripts (see Scripts/playground/run-script.js).</summary>
    private const string WorkerPath = "/js/script-worker.js";

    private const string DocumentCsp =
        "default-src 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "frame-ancestors 'none'; " +
        "form-action 'self'; " +
        "script-src 'self'; " +
        "worker-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https://assets.ppy.sh https://a.ppy.sh https://i.ytimg.com; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-src https://www.youtube-nocookie.com";

    // The playground worker only needs to load its own module and compile user scripts; it makes no
    // network calls and touches no DOM.
    private const string WorkerCsp = "default-src 'none'; script-src 'self' 'unsafe-eval'";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            bool isWorker = context.Request.Path.Equals(WorkerPath, StringComparison.OrdinalIgnoreCase);
            headers["Content-Security-Policy"] = isWorker ? WorkerCsp : DocumentCsp;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["X-Frame-Options"] = "DENY";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), browsing-topics=()";
            await next(context);
        });
}
