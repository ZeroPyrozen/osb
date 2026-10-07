using Microsoft.AspNetCore.Mvc;

namespace osb.Tests.Infrastructure;

/// <summary>A page that always fails, so the web tests can check the error page. Only exists in tests.</summary>
public class TestFailureController : Controller
{
    [HttpGet("/test/fail")]
    public IActionResult Fail() => throw new InvalidOperationException("A failure for the error page test.");
}
