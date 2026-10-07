using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using osb.Controllers;
using osb.Data;
using osb.Learn;
using osb.Tests.Infrastructure;
using Completion = osb.Controllers.LearnProgressController.CompletionDto;
using Progress = osb.Controllers.LearnProgressController.ProgressDto;

namespace osb.Tests.Controllers;

/// <summary>How saved progress is merged, against the real course. The web tests cover login and CSRF.</summary>
public sealed class LearnProgressControllerTests : IDisposable
{
    private static readonly CourseProvider Courses = new(new TestEnvironment(RepoPaths.Root), NullLogger<CourseProvider>.Instance);
    private static readonly LearnUnit[] Lessons = Courses.Current.Units.Where(u => u.Type == UnitType.Lesson).Take(3).ToArray();
    private static readonly LearnUnit Quiz = Courses.Current.Units.First(u => u.Type == UnitType.Quiz && u.Quiz!.Questions.Count >= 3);
    private static readonly DateTime March = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase database = new();

    public void Dispose() => database.Dispose();

    private LearnProgressController LoggedIn(int id = 42, string name = "Learner") => new(database.CreateContext(), Courses)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Name, name)], "test")),
            },
        },
    };

    private static Progress Send(params (string Unit, Completion Completion)[] completions) =>
        new(completions.ToDictionary(c => c.Unit, c => c.Completion));

    private async Task<Dictionary<string, Completion>> Save(LearnProgressController learner, params (string, Completion)[] completions) =>
        (await learner.Post(Send(completions), CancellationToken.None)).Value!.Completions;

    [Fact]
    public async Task UnknownUnits_AreIgnored()
    {
        var saved = await Save(LoggedIn(), ("no-such/unit", new Completion(March, null, null)), (Lessons[0].Id, new Completion(March, null, null)));

        Assert.Equal([Lessons[0].Id], saved.Keys);
    }

    [Fact]
    public async Task UnitIds_MatchIgnoringCase_AndAreSavedAsTheCourseWritesThem()
    {
        var saved = await Save(LoggedIn(), (Lessons[0].Id.ToUpperInvariant(), new Completion(March, null, null)));

        Assert.Equal([Lessons[0].Id], saved.Keys);
    }

    [Fact]
    public async Task TimesFromTheFutureOrBefore2020_BecomeNow()
    {
        var before = DateTime.UtcNow;

        var saved = await Save(LoggedIn(),
            (Lessons[0].Id, new Completion(DateTime.UtcNow.AddDays(1), null, null)),
            (Lessons[1].Id, new Completion(new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc), null, null)),
            (Lessons[2].Id, new Completion(March, null, null)));

        Assert.InRange(saved[Lessons[0].Id].CompletedAt, before, DateTime.UtcNow);
        Assert.InRange(saved[Lessons[1].Id].CompletedAt, before, DateTime.UtcNow);
        Assert.Equal(March, saved[Lessons[2].Id].CompletedAt);
    }

    [Fact]
    public async Task TheEarliestCompletion_IsKept()
    {
        await Save(LoggedIn(), (Lessons[0].Id, new Completion(March, null, null)));

        var earlier = await Save(LoggedIn(), (Lessons[0].Id, new Completion(March.AddDays(-10), null, null)));
        var later = await Save(LoggedIn(), (Lessons[0].Id, new Completion(March.AddDays(10), null, null)));

        Assert.Equal(March.AddDays(-10), earlier[Lessons[0].Id].CompletedAt);
        Assert.Equal(March.AddDays(-10), later[Lessons[0].Id].CompletedAt);
    }

    [Fact]
    public async Task TheBestQuizScore_IsKept()
    {
        int questions = Quiz.Quiz!.Questions.Count;

        Assert.Equal(2, (await Save(LoggedIn(), (Quiz.Id, new Completion(March, 2, questions))))[Quiz.Id].Score);
        Assert.Equal(2, (await Save(LoggedIn(), (Quiz.Id, new Completion(March, 1, questions))))[Quiz.Id].Score);
        var best = (await Save(LoggedIn(), (Quiz.Id, new Completion(March, 3, questions))))[Quiz.Id];

        Assert.Equal(3, best.Score);
        Assert.Equal(questions, best.MaxScore);
    }

    [Theory]
    [InlineData(100, null)]
    [InlineData(-3, 0)]
    public async Task QuizScores_StayBetweenZeroAndTheQuestionCount(int sent, int? expected)
    {
        int questions = Quiz.Quiz!.Questions.Count;

        var saved = (await Save(LoggedIn(), (Quiz.Id, new Completion(March, sent, 999))))[Quiz.Id];

        Assert.Equal(expected ?? questions, saved.Score);
        Assert.Equal(questions, saved.MaxScore);
    }

    [Fact]
    public async Task Scores_AreOnlyKeptForKnowledgeChecks()
    {
        var saved = (await Save(LoggedIn(), (Lessons[0].Id, new Completion(March, 5, 5))))[Lessons[0].Id];

        Assert.Null(saved.Score);
        Assert.Null(saved.MaxScore);
    }

    [Fact]
    public async Task MoreThanAThousandCompletions_AreRefused()
    {
        var tooMany = new Progress(Enumerable.Range(0, 1001).ToDictionary(i => $"unit/{i}", _ => new Completion(March, null, null)));

        var result = await LoggedIn().Post(tooMany, CancellationToken.None);

        var refusal = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Send at most 1000 completions at a time.", refusal.Value);
    }

    [Fact]
    public async Task TheFirstSave_AddsTheLearner_LaterOnesKeepTheirNameCurrent()
    {
        await Save(LoggedIn(42, "Learner"), (Lessons[0].Id, new Completion(March, null, null)));
        Learner? first;
        await using (var db = database.CreateContext())
            first = await db.Learners.FindAsync(42);

        await Save(LoggedIn(42, "Renamed"), (Lessons[1].Id, new Completion(March, null, null)));
        await using var check = database.CreateContext();
        var learner = (await check.Learners.FindAsync(42))!;

        Assert.Equal("Learner", first!.Username);
        Assert.Equal("Renamed", learner.Username);
        Assert.Equal(first.JoinedAt, learner.JoinedAt);
        Assert.True(learner.LastActiveAt >= first.LastActiveAt);
    }

    [Fact]
    public async Task Reading_ReturnsOnlyYourOwnCompletions_InUtc()
    {
        await Save(LoggedIn(42), (Lessons[0].Id, new Completion(March, null, null)));

        var mine = await LoggedIn(42).Get(CancellationToken.None);
        var theirs = await LoggedIn(43).Get(CancellationToken.None);

        var completion = Assert.Single(mine.Completions).Value;
        Assert.Equal(March, completion.CompletedAt);
        Assert.Equal(DateTimeKind.Utc, completion.CompletedAt.Kind);
        Assert.Empty(theirs.Completions);
    }
}
