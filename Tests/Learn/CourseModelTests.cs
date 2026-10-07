using osb.Learn;
using osb.Tests.Infrastructure;

namespace osb.Tests.Learn;

public class CourseModelTests
{
    private static CourseFolder TwoModules() => CourseFolder.WithModules("basics", "motion")
        .Module("basics")
        .Unit("basics", "01-intro.md", "title: Intro\ntype: lesson\nminutes: 4")
        .Unit("basics", "02-check.md", "title: Check\ntype: quiz\nminutes: 2\nquestions:\n  - { prompt: Yes?, choices: [Yes, No], answer: 0 }")
        .Module("motion")
        .Unit("motion", "01-move.md", "title: Move\ntype: exercise\nminutes: 6\nexercise: { solution: x, checks: [{ kind: parses }] }");

    [Fact]
    public void FindUnitAndFindModule_IgnoreCase()
    {
        using var folder = TwoModules();
        var course = folder.Load();

        Assert.Equal("basics/check", course.FindUnit("Basics/CHECK")?.Id);
        Assert.Equal("motion", course.FindModule("MOTION")?.Slug);
        Assert.Null(course.FindUnit("basics/missing"));
        Assert.Null(course.FindModule("missing"));
    }

    [Fact]
    public void Minutes_AddUpPerModulePathAndCourse()
    {
        using var folder = TwoModules();
        var course = folder.Load();

        Assert.Equal([6, 6], course.Modules.Select(m => m.Minutes));
        Assert.Equal(12, course.Paths[0].Minutes);
        Assert.Equal(3, course.Paths[0].UnitCount);
        Assert.Equal(12, course.TotalMinutes);
    }

    [Fact]
    public void UnitTypes_HaveTheirLabelsAndIcons()
    {
        using var folder = TwoModules();
        var units = folder.Load().Units;

        Assert.Equal(["Lesson", "Knowledge check", "Exercise"], units.Select(u => u.TypeLabel));
        Assert.Equal(["book", "quiz", "code"], units.Select(u => u.TypeIcon));
    }

    [Theory]
    [InlineData(UnitType.Lesson, 10)]
    [InlineData(UnitType.Quiz, 20)]
    [InlineData(UnitType.Exercise, 30)]
    public void XpRules_GiveXpByUnitType(UnitType type, int xp) =>
        Assert.Equal(xp, new XpRules(Lesson: 10, Quiz: 20, Exercise: 30, Module: 50, Path: 150, PerfectQuiz: 5).For(type));
}
