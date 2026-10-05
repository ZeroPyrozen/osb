#nullable enable

using Microsoft.AspNetCore.Html;

namespace osb.Helpers;

/// <summary>
/// Inline SVG icons drawn with <c>currentColor</c>, so Tailwind text colours apply to them.
/// Usage in Razor: <c>@Icons.Get("search", "size-4")</c>.
/// search, arrows, heart, logout, user, upload, discord and dots come from the Future redesign assets;
/// the rest are simple strokes drawn for this site in the same style.
/// </summary>
public static class Icons
{
    private static readonly Dictionary<string, (string ViewBox, string Body)> All = new()
    {
        ["search"] = ("0 0 19 19", """<path d="M13.4 13.49 16.15 16.15M9.03 5.7a2.85 2.85 0 0 1 2.85 2.85M15.26 9.06a6.21 6.21 0 1 1-12.41 0 6.21 6.21 0 0 1 12.41 0Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" fill="none"/>"""),
        ["arrow-left"] = ("0 0 40 40", """<path d="M24 28 16 20l8-8" stroke="currentColor" stroke-width="4" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["arrow-right"] = ("0 0 40 40", """<path d="m16 12 8 8-8 8" stroke="currentColor" stroke-width="4" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["heart"] = ("0 0 15 14", """<path d="M8.06 1.73 7.5 2.32l-.56-.59C5.4.09 2.89.09 1.35 1.73c-1.55 1.64-1.55 4.3 0 5.94l5.03 5.34c.62.66 1.62.66 2.24 0l5.03-5.34c1.55-1.64 1.55-4.3 0-5.94-1.54-1.64-4.05-1.64-5.59 0Z" fill="currentColor"/>"""),
        ["logout"] = ("0 0 15 15", """<path d="M9.15 4.88V3.56a1.31 1.31 0 0 0-1.32-1.31H3.2a1.31 1.31 0 0 0-1.33 1.31v7.88a1.31 1.31 0 0 0 1.33 1.31h4.63a1.31 1.31 0 0 0 1.32-1.31v-1.32M5.18 7.5h7.95m0 0-1.99-1.97m1.99 1.97-1.99 1.97" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["user"] = ("0 0 15 15", """<path d="M1.5 12.82c0-2.36 1.97-4.27 6-4.27s6 1.91 6 4.27c0 .38-.27.68-.61.68H2.11c-.34 0-.61-.3-.61-.68ZM9.75 3.75a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Z" stroke="currentColor" stroke-width="1.6" fill="none"/>"""),
        ["upload"] = ("0 0 17 15", """<path d="M3.85 10.35c-1.22 0-2.2-.83-2.2-2.05a2.2 2.2 0 0 1 2.45-2.19A3.92 3.92 0 0 1 11.43 3.68a3.43 3.43 0 0 1 1.47 6.53M8.39 13.31V7.69m0 0L6.14 10m2.25-2.31L10.64 10" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["discord"] = ("0 0 17 13", """<path d="M13.99 1.51c1.79 2.62 2.67 5.57 2.34 8.96a.06.06 0 0 1-.02.04c-1.35.99-2.66 1.59-3.95 1.99a.05.05 0 0 1-.06-.02 10.47 10.47 0 0 1-.81-1.31.05.05 0 0 1 .03-.07c.43-.16.84-.36 1.23-.58a.05.05 0 0 0 0-.08 4.53 4.53 0 0 1-.24-.19.05.05 0 0 0-.05-.01 8.48 8.48 0 0 1-7.94 0 .05.05 0 0 0-.05.01l-.24.19a.05.05 0 0 0 0 .08c.39.22.8.42 1.23.59a.05.05 0 0 1 .03.07c-.23.46-.5.89-.81 1.31a.05.05 0 0 1-.05.02c-1.29-.4-2.59-1-3.94-1.99a.06.06 0 0 1-.02-.04C.4 7.54.96 4.56 3 1.51l.02-.02A13.27 13.27 0 0 1 6.24.5a.05.05 0 0 1 .05.03c.14.24.3.56.41.82a12.27 12.27 0 0 1 3.6 0c.11-.25.26-.57.4-.82a.05.05 0 0 1 .06-.03 13.2 13.2 0 0 1 3.21 1Zm-6.69 5.59c.01-.87-.62-1.59-1.42-1.59-.79 0-1.42.71-1.42 1.59 0 .87.64 1.59 1.42 1.59.79 0 1.42-.72 1.42-1.59Zm5.25 0c.01-.87-.62-1.59-1.42-1.59-.79 0-1.42.71-1.42 1.59 0 .87.64 1.59 1.42 1.59.8 0 1.42-.72 1.42-1.59Z" fill="currentColor"/>"""),
        ["dots"] = ("0 0 19 19", """<circle cx="3.8" cy="9.5" r="1.9" fill="currentColor"/><circle cx="9.5" cy="9.5" r="1.9" fill="currentColor"/><circle cx="15.2" cy="9.5" r="1.9" fill="currentColor"/>"""),

        ["menu"] = ("0 0 24 24", """<path d="M4 7h16M4 12h16M4 17h16" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/>"""),
        ["close"] = ("0 0 24 24", """<path d="m6 6 12 12M18 6 6 18" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/>"""),
        ["check"] = ("0 0 24 24", """<path d="m5 12.5 4.5 4.5L19 7.5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["chevron-right"] = ("0 0 24 24", """<path d="m9 6 6 6-6 6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["chevron-down"] = ("0 0 24 24", """<path d="m6 9 6 6 6-6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["external"] = ("0 0 24 24", """<path d="M14 4h6v6M20 4l-9 9M18 14v5a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V7a1 1 0 0 1 1-1h5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["download"] = ("0 0 24 24", """<path d="M12 4v11m0 0-4.5-4.5M12 15l4.5-4.5M5 19h14" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["play"] = ("0 0 24 24", """<path d="M8 5.5v13a1 1 0 0 0 1.52.85l10.5-6.5a1 1 0 0 0 0-1.7L9.52 4.65A1 1 0 0 0 8 5.5Z" fill="currentColor"/>"""),
        ["pause"] = ("0 0 24 24", """<rect x="6" y="5" width="4" height="14" rx="1" fill="currentColor"/><rect x="14" y="5" width="4" height="14" rx="1" fill="currentColor"/>"""),
        ["restart"] = ("0 0 24 24", """<path d="M4 12a8 8 0 1 0 2.34-5.66M4 4v4.5h4.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["clock"] = ("0 0 24 24", """<circle cx="12" cy="12" r="8.5" stroke="currentColor" stroke-width="2" fill="none"/><path d="M12 7.5V12l3 2" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/>"""),
        ["book"] = ("0 0 24 24", """<path d="M5 4.5h10.5A2.5 2.5 0 0 1 18 7v12.5H7.5A2.5 2.5 0 0 1 5 17V4.5Zm0 12.5a2.5 2.5 0 0 1 2.5-2.5H18" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/>"""),
        ["quiz"] = ("0 0 24 24", """<circle cx="12" cy="12" r="8.5" stroke="currentColor" stroke-width="2" fill="none"/><path d="M9.6 9.4a2.5 2.5 0 1 1 3.4 2.33c-.6.25-1 .8-1 1.45v.32" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/><circle cx="12" cy="16.6" r="1.15" fill="currentColor"/>"""),
        ["code"] = ("0 0 24 24", """<path d="m8.5 7-5 5 5 5M15.5 7l5 5-5 5M13.5 4.5l-3 15" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["lock"] = ("0 0 24 24", """<rect x="5" y="10.5" width="14" height="9.5" rx="2" stroke="currentColor" stroke-width="2" fill="none"/><path d="M8.5 10.5V8a3.5 3.5 0 0 1 7 0v2.5" stroke="currentColor" stroke-width="2" fill="none"/>"""),
        ["star"] = ("0 0 24 24", """<path d="m12 3.5 2.6 5.27 5.82.85-4.21 4.1.99 5.79L12 16.77l-5.2 2.74.99-5.79-4.21-4.1 5.82-.85L12 3.5Z" fill="currentColor"/>"""),
        ["trophy"] = ("0 0 24 24", """<path d="M8 4.5h8v5a4 4 0 0 1-8 0v-5ZM8 6.5H5a3 3 0 0 0 3 3.5M16 6.5h3a3 3 0 0 1-3 3.5M12 13.5V17m-3.5 2.5h7M9.5 17h5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["flame"] = ("0 0 24 24", """<path d="M12 21c-3.6 0-6-2.4-6-5.6 0-3.2 2.4-4.8 3.4-7.9.8 1.2 1.3 2.3 1.4 3.6 1.4-1.5 2.2-3.7 2-6.6 3.3 2.3 5.2 6 5.2 9.9 0 4.2-2.6 6.6-6 6.6Z" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/>"""),
        ["sparkles"] = ("0 0 24 24", """<path d="M10 3.5c.6 3.6 2.4 5.4 6 6-3.6.6-5.4 2.4-6 6-.6-3.6-2.4-5.4-6-6 3.6-.6 5.4-2.4 6-6ZM17.5 14c.3 1.8 1.2 2.7 3 3-1.8.3-2.7 1.2-3 3-.3-1.8-1.2-2.7-3-3 1.8-.3 2.7-1.2 3-3Z" fill="currentColor"/>"""),
        ["map"] = ("0 0 24 24", """<path d="m9 5-5 2v12l5-2 6 2 5-2V5l-5 2-6-2Zm0 0v12m6-10v12" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/>"""),
        ["layers"] = ("0 0 24 24", """<path d="m12 4 8.5 4.5L12 13 3.5 8.5 12 4Zm-8.5 8L12 16.5l8.5-4.5m-17 3.5L12 20l8.5-4.5" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/>"""),
        ["info"] = ("0 0 24 24", """<circle cx="12" cy="12" r="8.5" stroke="currentColor" stroke-width="2" fill="none"/><path d="M12 11v5.5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/><circle cx="12" cy="7.7" r="1.2" fill="currentColor"/>"""),
        ["warning"] = ("0 0 24 24", """<path d="M10.3 4.6 3.4 17a2 2 0 0 0 1.74 3h13.72a2 2 0 0 0 1.74-3L13.7 4.6a1.95 1.95 0 0 0-3.4 0Z" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/><path d="M12 9.5v4.5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/><circle cx="12" cy="17" r="1.15" fill="currentColor"/>"""),
        ["grid"] = ("0 0 24 24", """<rect x="4" y="4" width="6.5" height="6.5" rx="1.5" stroke="currentColor" stroke-width="2" fill="none"/><rect x="13.5" y="4" width="6.5" height="6.5" rx="1.5" stroke="currentColor" stroke-width="2" fill="none"/><rect x="4" y="13.5" width="6.5" height="6.5" rx="1.5" stroke="currentColor" stroke-width="2" fill="none"/><rect x="13.5" y="13.5" width="6.5" height="6.5" rx="1.5" stroke="currentColor" stroke-width="2" fill="none"/>"""),
        ["eye"] = ("0 0 24 24", """<path d="M2.5 12S6 5.5 12 5.5 21.5 12 21.5 12 18 18.5 12 18.5 2.5 12 2.5 12Z" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/><circle cx="12" cy="12" r="2.8" stroke="currentColor" stroke-width="2" fill="none"/>"""),
        ["image"] = ("0 0 24 24", """<rect x="3.5" y="5" width="17" height="14" rx="2" stroke="currentColor" stroke-width="2" fill="none"/><circle cx="9" cy="10" r="1.6" fill="currentColor"/><path d="m4.5 17.5 5-5 3.5 3.5 2.5-2.5 4 4" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/>"""),
        ["curve"] = ("0 0 24 24", """<path d="M4 19.5c7 0 9-15 16-15" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/><circle cx="4" cy="19.5" r="1.7" fill="currentColor"/><circle cx="20" cy="4.5" r="1.7" fill="currentColor"/>"""),
        ["scissors"] = ("0 0 24 24", """<circle cx="6.5" cy="7" r="2.6" stroke="currentColor" stroke-width="2" fill="none"/><circle cx="6.5" cy="17" r="2.6" stroke="currentColor" stroke-width="2" fill="none"/><path d="M8.7 8.5 19.5 17M8.7 15.5 19.5 7" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/>"""),
        ["repeat"] = ("0 0 24 24", """<path d="m17 3.5 3 3-3 3M20 6.5H8.5A4.5 4.5 0 0 0 4 11v1M7 20.5l-3-3 3-3M4 17.5h11.5A4.5 4.5 0 0 0 20 13v-1" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/>"""),
        ["film"] = ("0 0 24 24", """<rect x="4.5" y="3.5" width="15" height="17" rx="2" stroke="currentColor" stroke-width="2" fill="none"/><path d="M8.5 3.5v17M15.5 3.5v17M4.5 8h4M4.5 12h4M4.5 16h4M15.5 8h4M15.5 12h4M15.5 16h4" stroke="currentColor" stroke-width="2" fill="none"/>"""),
        ["music"] = ("0 0 24 24", """<path d="M9 17.5V5.5l10.5-2v12" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/><circle cx="6.5" cy="17.5" r="2.5" stroke="currentColor" stroke-width="2" fill="none"/><circle cx="17" cy="15.5" r="2.5" stroke="currentColor" stroke-width="2" fill="none"/>"""),
        ["gamepad"] = ("0 0 24 24", """<path d="M7.5 7.5h9a4.5 4.5 0 0 1 4.5 4.5v2.2a2.8 2.8 0 0 1-5.1 1.6L14.6 14H9.4l-1.3 1.8A2.8 2.8 0 0 1 3 14.2V12a4.5 4.5 0 0 1 4.5-4.5Z" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/><path d="M8 9.8v3.4M6.3 11.5h3.4" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/><circle cx="15.6" cy="10.6" r="1.1" fill="currentColor"/><circle cx="17.6" cy="12.6" r="1.1" fill="currentColor"/>"""),
        ["bolt"] = ("0 0 24 24", """<path d="M13.5 3 5.5 13.5h6L10.5 21l8-10.5h-6L13.5 3Z" stroke="currentColor" stroke-width="2" stroke-linejoin="round" fill="none"/>"""),
        ["particles"] = ("0 0 24 24", """<circle cx="6" cy="7" r="2" fill="currentColor"/><circle cx="15" cy="5" r="1.3" fill="currentColor"/><circle cx="18.5" cy="12" r="2.4" fill="currentColor"/><circle cx="9.5" cy="13.5" r="1.6" fill="currentColor"/><circle cx="13.5" cy="19" r="1.9" fill="currentColor"/><circle cx="5" cy="18.5" r="1.1" fill="currentColor"/>"""),
        ["gauge"] = ("0 0 24 24", """<path d="M4.2 17a8.5 8.5 0 1 1 15.6 0" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/><path d="m12 14 4-4.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" fill="none"/><circle cx="12" cy="14" r="1.7" fill="currentColor"/>"""),
    };

    public static bool Exists(string name) => All.ContainsKey(name);

    public static IHtmlContent Get(string name, string cssClass = "size-4")
    {
        if (!All.TryGetValue(name, out var icon))
            throw new ArgumentException($"Unknown icon '{name}'.", nameof(name));
        return new HtmlString($"""<svg class="{cssClass}" viewBox="{icon.ViewBox}" aria-hidden="true" focusable="false" xmlns="http://www.w3.org/2000/svg">{icon.Body}</svg>""");
    }
}
