using Microsoft.Extensions.FileProviders;

namespace osb.Tests.Infrastructure;

/// <summary>The hosting environment for classes that take an <see cref="IWebHostEnvironment"/>.</summary>
public sealed class TestEnvironment(string contentRoot, string environmentName = "Production") : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "osb";
    public string EnvironmentName { get; set; } = environmentName;
    public string ContentRootPath { get; set; } = contentRoot;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = Path.Combine(contentRoot, "wwwroot");
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}
