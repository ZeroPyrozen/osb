using osb.Helpers;

namespace osb.Tests.Helpers;

public class FormatTests
{
    [Theory]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ?autoplay=1", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=42", "dQw4w9WgXcQ")]
    [InlineData("https://m.youtube.com/watch?v=a-b_c1D2e3F", "a-b_c1D2e3F")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    public void YouTubeId_FindsTheVideoInYouTubeLinks(string url, string expected) =>
        Assert.Equal(expected, Format.YouTubeId(url));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://vimeo.com/123456789")]
    [InlineData("https://www.youtube.com/embed/short")]
    [InlineData("https://www.youtube.com/channel/UCabcdefghijklmnop")]
    public void YouTubeId_IsNullForAnythingElse(string? url) =>
        Assert.Null(Format.YouTubeId(url));
}
