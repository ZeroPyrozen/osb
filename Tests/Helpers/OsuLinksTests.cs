using osb.Helpers;

namespace osb.Tests.Helpers;

public class OsuLinksTests
{
    [Theory]
    [InlineData("1011020", 1011020, false)]
    [InlineData("  1011020 ", 1011020, false)]
    [InlineData("https://osu.ppy.sh/beatmapsets/1011020", 1011020, false)]
    [InlineData("https://osu.ppy.sh/beatmapsets/1011020#osu/2115170", 1011020, false)]
    [InlineData("osu.ppy.sh/beatmapsets/1011020/discussion", 1011020, false)]
    [InlineData("http://osu.ppy.sh/s/1011020", 1011020, false)]
    [InlineData("https://old.ppy.sh/s/1011020", 1011020, false)]
    [InlineData("HTTPS://OSU.PPY.SH/BEATMAPSETS/1011020", 1011020, false)]
    [InlineData("https://osu.ppy.sh/beatmaps/2115170", 2115170, true)]
    [InlineData("https://osu.ppy.sh/b/2115170?m=0", 2115170, true)]
    public void BeatmapLinks_GiveTheBeatmapsetOrDifficulty(string input, int id, bool isDifficulty) =>
        Assert.Equal(new BeatmapLink(id, isDifficulty), OsuLinks.ParseBeatmap(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("12abc")]
    [InlineData("Blue Zenith")]
    [InlineData("https://example.com/beatmapsets/1011020")]
    [InlineData("https://osu.ppy.sh/users/2")]
    [InlineData("https://osu.ppy.sh/beatmapsets/")]
    [InlineData("https://osu.ppy.sh/beatmapsets/99999999999")]
    public void AnythingElse_IsNotABeatmap(string? input) =>
        Assert.Null(OsuLinks.ParseBeatmap(input));

    [Fact]
    public void People_AreReadFromNamesIdsAndProfileLinks_InOrder()
    {
        string list = "Alice, osu.ppy.sh/users/2\nhttps://osu.ppy.sh/u/727 , @bob,,  Some Name \r\nhttps://osu.ppy.sh/users/Ann%20Marie/osu";

        Assert.Equal(["Alice", "2", "727", "bob", "Some Name", "Ann Marie"], OsuLinks.ParseUsers(list));
    }

    [Fact]
    public void EachPerson_IsListedOnce_WhateverTheCase() =>
        Assert.Equal(["Alice", "2"], OsuLinks.ParseUsers("Alice, alice, 2, ALICE, osu.ppy.sh/users/2"));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" , \n ,")]
    [InlineData("@")]
    public void NoNames_IsNobody(string? input) =>
        Assert.Empty(OsuLinks.ParseUsers(input));
}
