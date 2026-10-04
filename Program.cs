using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.Learn;
using osb.Services;

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Lets the app run as a systemd service (Type=notify) with journald-friendly logs on the
// Raspberry Pi. Does nothing when the app isn't started by systemd.
builder.Services.AddSystemd();

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
    policy =>
    {
        // An origin never ends with "/", so "https://osu.ppy.sh/" could never match
        policy.WithOrigins("https://osu.ppy.sh");
    });
});
builder.Services.AddHttpClient<IOsuWebHelper, OsuWebHelper>(client => client.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddHealthChecks();

// osu! logins (see AuthController) are kept in a 30-day sliding cookie.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "osb.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/auth";
        options.ReturnUrlParameter = "returnUrl";
        // API calls get a 401 instead of a redirect to the osu! login page.
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            else
                context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// SQLite database for the showcase and learner progress. A relative Data Source is resolved
// against the content root; on the Pi the systemd unit points it at /var/lib/osb.
string dbConnection = ResolveSqlitePath(
    builder.Configuration.GetConnectionString("Osb") ?? "Data Source=App_Data/osb.db",
    builder.Environment.ContentRootPath);
builder.Services.AddDbContext<OsbDbContext>(options => options
    .UseSqlite(dbConnection)
    .UseSeeding((context, _) => ShowcaseSeeder.Seed(context))
    .UseAsyncSeeding((context, _, ct) => ShowcaseSeeder.SeedAsync(context, ct)));
builder.Services.AddScoped<ShowcaseService>();

// osb! learn: the course is read from Content/Learn at startup (see Learn/CourseLoader.cs).
builder.Services.AddSingleton<CourseProvider>();

// Set by the systemd unit on the Pi. A fixed key folder and application name keep login
// cookies valid across restarts and deploys (every deploy runs from a new release folder).
string keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrEmpty(keysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("osb");
}

var app = builder.Build();

// Bring the database schema up to date (and insert any new seed data) before serving requests.
await using (var scope = app.Services.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Database.MigrateAsync();

// Load the learn course now, so broken content fails the start (and a deploy's health check).
app.Services.GetRequiredService<CourseProvider>();

// Behind a reverse proxy on the same machine (nginx, Caddy, cloudflared), use the original
// client IP and scheme, so HTTPS redirection, HSTS and secure cookies behave correctly.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Friendly error pages for people; API calls keep their plain status codes.
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/Error/{0}"));
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

// wwwroot files, precompressed (gzip/brotli) and cache-validated at build time.
app.MapStaticAssets();
app.MapHealthChecks("/healthz");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string ResolveSqlitePath(string connectionString, string contentRoot)
{
    var csb = new SqliteConnectionStringBuilder(connectionString);
    if (!string.IsNullOrEmpty(csb.DataSource) && csb.DataSource != ":memory:" && !Path.IsPathRooted(csb.DataSource))
        csb.DataSource = Path.GetFullPath(Path.Combine(contentRoot, csb.DataSource));
    if (Path.GetDirectoryName(csb.DataSource) is { Length: > 0 } folder)
        Directory.CreateDirectory(folder);
    return csb.ToString();
}
