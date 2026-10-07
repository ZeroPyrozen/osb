using Microsoft.EntityFrameworkCore;
using osb.Data;
using osb.Helpers;
using osb.Tests.Infrastructure;

namespace osb.Tests.Web;

/// <summary>Profile pages add live osu! data when osu! has it, and still work when it doesn't.</summary>
public class CommunityProfileTests(OsbWebFactory site) : IClassFixture<OsbWebFactory>
{
    private async Task<int> MemberAsync(int skip)
    {
        using var scope = site.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<OsbDbContext>().Users
            .Where(u => u.IsCommunityMember).OrderBy(u => u.Id).Skip(skip).Select(u => u.Id).FirstAsync();
    }

    [Fact]
    public async Task WithOsuData_ShowsTheCoverAndTheCountry()
    {
        int id = await MemberAsync(0);
        site.Osu.Profiles[id] = new WebUserModel
        {
            ID = id,
            Cover = new WebCover { URL = "https://assets.ppy.sh/user-cover.jpg" },
            CountryCode = "JP",
            Country = new WebCountry { Code = "JP", Name = "Japan" },
        };

        string html = await site.CreateClient().GetStringAsync($"/community/profile?userID={id}");

        Assert.Contains("https://assets.ppy.sh/user-cover.jpg", html);
        Assert.Contains("/images/flags/JP.svg", html);
        Assert.Contains("Japan", html);
    }

    [Fact]
    public async Task WithoutOsuData_UsesOneOfTheSitesBanners()
    {
        int id = await MemberAsync(1);

        string html = await site.CreateClient().GetStringAsync($"/community/profile?userID={id}");

        Assert.Contains($"/images/banners/bg-0{id % 5 + 1}.webp", html);
        Assert.DoesNotContain("/images/flags/", html);
    }

    [Fact]
    public async Task ACountryWithoutAFlag_ShowsJustItsName()
    {
        int id = await MemberAsync(2);
        site.Osu.Profiles[id] = new WebUserModel { ID = id, CountryCode = "ZZ", Country = new WebCountry { Code = "ZZ", Name = "Nowhere" } };

        string html = await site.CreateClient().GetStringAsync($"/community/profile?userID={id}");

        Assert.Contains("Nowhere", html);
        Assert.DoesNotContain("/images/flags/", html);
    }
}
