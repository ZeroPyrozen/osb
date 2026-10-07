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
    [InlineData("https://youtu.be/dQw4w9WgXcQ?si=abc123", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?feature=share&v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/live/dQw4w9WgXcQ?feature=shared", "dQw4w9WgXcQ")]
    public void YouTubeId_FindsTheVideoInYouTubeLinks(string url, string expected) =>
        Assert.Equal(expected, Format.YouTubeId(url));

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?si=abc123")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PL123")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    public void YouTubeEmbedUrl_IsTheLinkTheSiteStores(string url) =>
        Assert.Equal("https://www.youtube.com/embed/dQw4w9WgXcQ", Format.YouTubeEmbedUrl(url));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://vimeo.com/123456789")]
    [InlineData("https://www.youtube.com/embed/short")]
    [InlineData("https://www.youtube.com/channel/UCabcdefghijklmnop")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQtoolong")]
    public void YouTubeId_IsNullForAnythingElse(string? url)
    {
        Assert.Null(Format.YouTubeId(url));
        Assert.Null(Format.YouTubeEmbedUrl(url));
    }
}
