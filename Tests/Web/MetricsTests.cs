using System.Net;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>The site with Prometheus metrics turned on, to check the /metrics endpoint.</summary>
public sealed class MetricsSite : OsbWebFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Metrics:Enabled", "true");
    }
}

public class MetricsTests(MetricsSite site) : IClassFixture<MetricsSite>
{
    [Fact]
    public async Task Metrics_AreExposed_InPrometheusFormat()
    {
        var client = site.CreateClient();
        await client.GetAsync("/healthz"); // one request first, so the HTTP counter has a series to show

        var response = await client.GetAsync("/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("http_requests_received_total", body);
    }
}
