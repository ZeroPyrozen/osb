using osb.Services;
using osb.ViewModels;

namespace osb.Tests.ViewModels;

public class ShowcaseIndexViewModelTests
{
    private static ShowcaseIndexViewModel Page(string? query = null, string? tag = null, string? medium = null) =>
        new(new ShowcasePage([], 0, 1, 12), query, tag, medium, [], []);

    [Fact]
    public void Url_WithoutFiltersIsTheShowcase() =>
        Assert.Equal("/showcase", Page().Url());

    [Fact]
    public void Url_KeepsTheCurrentFiltersEscaped() =>
        Assert.Equal("/showcase?s=blue%20zenith&t=particles&m=C%23", Page("blue zenith", "particles", "C#").Url());

    [Fact]
    public void Url_LeavesOutPageOne()
    {
        var page = Page("xi");

        Assert.Equal("/showcase?s=xi", page.Url(page: 1));
        Assert.Equal("/showcase?s=xi&page=3", page.Url(page: 3));
    }

    [Fact]
    public void Url_ChangesOrClearsOneFilterAndKeepsTheRest()
    {
        var page = Page(tag: "particles", medium: "SGL");

        Assert.Equal("/showcase?t=lyrics&m=SGL", page.Url(tag: "lyrics"));
        Assert.Equal("/showcase?m=SGL", page.Url(tag: null));
        Assert.Equal("/showcase?t=particles", page.Url(medium: null));
    }

    [Theory]
    [InlineData(null, null, null, false)]
    [InlineData("  ", "", null, false)]
    [InlineData("xi", null, null, true)]
    [InlineData(null, "lyrics", null, true)]
    [InlineData(null, null, "SGL", true)]
    public void IsFiltered_WhenAnyFilterHasAValue(string? query, string? tag, string? medium, bool expected) =>
        Assert.Equal(expected, Page(query, tag, medium).IsFiltered);
}
