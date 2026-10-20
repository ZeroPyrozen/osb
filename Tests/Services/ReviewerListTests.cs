using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using osb.Services;

namespace osb.Tests.Services;

public class ReviewerListTests
{
    private static ReviewerList List(string? setting) => new(
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Showcase:Reviewers"] = setting }).Build(),
        NullLogger<ReviewerList>.Instance);

    private static ClaimsPrincipal LoggedIn(int id) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Name, "someone")], "Cookies"));

    [Theory]
    [InlineData("2", 2, true)]
    [InlineData(" 2 , 1234567 ", 1234567, true)]
    [InlineData("2;3 4", 4, true)]
    [InlineData("peppy, 2, -5", 2, true)]
    [InlineData("2", 3, false)]
    [InlineData("", 2, false)]
    [InlineData(null, 2, false)]
    public void TheSetting_ListsOsuUserIds_AndSkipsAnythingElse(string? setting, int id, bool reviewer) =>
        Assert.Equal(reviewer, List(setting).Contains(id));

    [Fact]
    public async Task Reviewers_GetTheReviewerRole_OnACopyOfTheirLogin()
    {
        var login = LoggedIn(2);

        var transformed = await new ReviewerClaims(List("2")).TransformAsync(login);

        Assert.True(transformed.IsInRole(ReviewerList.Role));
        Assert.Equal("2", transformed.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.False(login.IsInRole(ReviewerList.Role));
    }

    [Fact]
    public async Task OtherMembersAndVisitors_DontGetIt()
    {
        var claims = new ReviewerClaims(List("2"));

        Assert.False((await claims.TransformAsync(LoggedIn(3))).IsInRole(ReviewerList.Role));
        Assert.False((await claims.TransformAsync(new ClaimsPrincipal(new ClaimsIdentity()))).IsInRole(ReviewerList.Role));
    }

    [Fact]
    public async Task TheRole_IsAddedOnce_HoweverOftenTheLoginIsRead()
    {
        var claims = new ReviewerClaims(List("2"));

        var twice = await claims.TransformAsync(await claims.TransformAsync(LoggedIn(2)));

        Assert.Single(twice.FindAll(ClaimTypes.Role));
    }
}
