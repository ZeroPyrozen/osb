using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection.Extensions;
using osb.Helpers;

namespace osb.Tests.Infrastructure;

/// <summary>
/// The whole site in memory, configured as it runs on the Pi (Production), with its own temporary
/// SQLite database and a fake osu! (<see cref="Osu"/>). The tests of one class share an instance.
/// </summary>
public class OsbWebFactory : WebApplicationFactory<Program>
{
    /// <summary>The osu! user ID the site treats as a reviewer (its Showcase:Reviewers setting).</summary>
    public const int ReviewerId = 9001;

    private readonly string folder = Directory.CreateTempSubdirectory("osb-web-").FullName;

    public FakeOsu Osu { get; } = new();

    public string DatabasePath => Path.Combine(folder, "osb.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting("ConnectionStrings:Osb", $"Data Source={DatabasePath}");
        builder.UseSetting("Showcase:Reviewers", ReviewerId.ToString());
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IOsuWebHelper>();
            services.AddSingleton<IOsuWebHelper>(Osu);
            services.AddSingleton<IStartupFilter, LoopbackClientFilter>();
            // Adds TestFailureController, which throws, for testing the error page.
            services.AddControllers().AddApplicationPart(typeof(OsbWebFactory).Assembly);
        });
    }

    /// <summary>A client that keeps cookies and shows redirects instead of following them.</summary>
    public HttpClient CreateBrowser() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(folder, recursive: true);
        }
        catch (IOException)
        {
            // Left for the OS to clean up with the rest of the temp folder.
        }
    }

    /// <summary>
    /// Test requests have no client address. On the Pi, requests reach the site from cloudflared or
    /// nginx on the same machine, so give them the loopback address and forwarded headers are
    /// trusted as they would be there.
    /// </summary>
    private sealed class LoopbackClientFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress ??= IPAddress.Loopback;
                return nextMiddleware(context);
            });
            next(app);
        };
    }
}
