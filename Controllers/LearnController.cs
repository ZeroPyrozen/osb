#nullable enable

using Microsoft.AspNetCore.Mvc;
using osb.Learn;

namespace osb.Controllers;

/// <summary>osb! learn: the course catalog, modules, units, the playground and the progress page.</summary>
public class LearnController(CourseProvider courses) : Controller
{
    [HttpGet("/learn")]
    public IActionResult Index() => View(courses.Current);

    [HttpGet("/learn/playground")]
    public IActionResult Playground() => View();

    [HttpGet("/learn/progress")]
    public IActionResult Progress() => View(courses.Current);

    [HttpGet("/learn/{module}")]
    public IActionResult Module(string module) =>
        courses.Current.FindModule(module) is { } found ? View(found) : NotFound();

    [HttpGet("/learn/{module}/{unit}")]
    public IActionResult Unit(string module, string unit) =>
        courses.Current.FindUnit($"{module}/{unit}") is { } found ? View(found) : NotFound();
}
