using osb.Services;

namespace osb.Tests.Services;

/// <summary>The pager's page numbers. 0 stands for a gap ("…").</summary>
public class ShowcasePageTests
{
    private static int[] Links(int page, int pageCount) =>
        new ShowcasePage([], pageCount * 12, page, 12).PagerLinks.Select(p => p ?? 0).ToArray();

    [Theory]
    [InlineData(1, new[] { 1, 2, 3, 4, 5, 0, 21 })]
    [InlineData(3, new[] { 1, 2, 3, 4, 5, 0, 21 })]
    [InlineData(4, new[] { 1, 2, 3, 4, 5, 6, 0, 21 })]
    [InlineData(11, new[] { 1, 0, 9, 10, 11, 12, 13, 0, 21 })]
    [InlineData(19, new[] { 1, 0, 17, 18, 19, 20, 21 })]
    [InlineData(21, new[] { 1, 0, 17, 18, 19, 20, 21 })]
    public void FivePagesAroundTheCurrentOne_AndTheFirstAndLast(int page, int[] expected) =>
        Assert.Equal(expected, Links(page, 21));

    [Theory]
    [InlineData(5, 21, new[] { 1, 2, 3, 4, 5, 6, 7, 0, 21 })]
    [InlineData(17, 21, new[] { 1, 0, 15, 16, 17, 18, 19, 20, 21 })]
    [InlineData(1, 7, new[] { 1, 2, 3, 4, 5, 6, 7 })]
    public void AGap_NeverHidesJustOnePage(int page, int pageCount, int[] expected) =>
        Assert.Equal(expected, Links(page, pageCount));

    [Theory]
    [InlineData(1, new[] { 1 })]
    [InlineData(2, new[] { 1, 2 })]
    [InlineData(5, new[] { 1, 2, 3, 4, 5 })]
    public void AFewPages_AreAllShown(int pageCount, int[] expected) =>
        Assert.Equal(expected, Links(1, pageCount));
}
