using System.Net;
using System.Text;

namespace osb.Tests.Infrastructure;

/// <summary>Answers HTTP requests from a function and records what was sent, for testing API clients.</summary>
public sealed class StubHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public sealed record Request(HttpMethod Method, string Url, string? Body, string? Authorization);

    public List<Request> Requests { get; } = new();

    private readonly object gate = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        string? body = request.Content != null ? await request.Content.ReadAsStringAsync(ct) : null;
        // AbsoluteUri keeps escapes such as %20 as sent; ToString() would show them unescaped.
        var entry = new Request(request.Method, request.RequestUri!.AbsoluteUri, body, request.Headers.Authorization?.ToString());
        lock (gate)
            Requests.Add(entry);
        return respond(request);
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}
