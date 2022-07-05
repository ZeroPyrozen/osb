using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace osb.Helpers
{
    public interface IOsuWebHelper
    {
        public string GetAuthorizationCode();
        public Task<TokenModel> GenerateAccessTokenAuthCode(string code);
        public Task<TokenModel> GenerateAccessTokenClient();
        public Task<WebUserModel> GetOwnData(string token);
        public Task<WebUserModel> GetUserData(string token, string userID);
    }

    public class OsuWebHelper : IOsuWebHelper
    {
        private string baseURL = "https://osu.ppy.sh/";
        private HttpClient client = new HttpClient();

        private readonly IConfiguration Configuration;

        public OsuWebHelper(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string GetAuthorizationCode()
        {
            string URL = Configuration.GetSection("API")["AuthURL"];
            string clientID = Configuration.GetSection("API")["ClientID"];
            string redirectURI = Configuration.GetSection("API")["RedirectURL"];
            URL = URL.Replace(":client_id", clientID).Replace(":redirect_uri", redirectURI).Replace(":scope", "identify");
            return URL;
        }

        public async Task<TokenModel> GenerateAccessTokenAuthCode(string code)
        {
            AuthCodeGrantModel authData = new AuthCodeGrantModel();
            authData.ClientID = int.Parse(Configuration.GetSection("API")["ClientID"]);
            authData.ClientSecret = Configuration.GetSection("API")["ClientSecret"];
            authData.GrantType = "authorization_code";
            authData.Code = code;
            authData.RedirectURI = Configuration.GetSection("API")["RedirectURL"];
            string URL = Configuration.GetSection("API")["TokenURL"];
            HttpResponseMessage response = await client.PostAsJsonAsync(URL, authData);
            TokenModel result = new TokenModel();
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadFromJsonAsync<TokenModel>();
                    result.GeneratedOn = DateTime.Now;
                }
            }
            catch
            {
                return null;
            }

            return result;
        }

        public async Task<TokenModel> GenerateAccessTokenClient()
        {
            ClientGrantModel clientData = new ClientGrantModel();
            clientData.ClientID = int.Parse(Configuration.GetSection("API")["ClientID"]);
            clientData.ClientSecret = Configuration.GetSection("API")["ClientSecret"];
            clientData.GrantType = "client_credentials";
            clientData.Scope = "public";
            string URL = Configuration.GetSection("API")["TokenURL"];
            HttpResponseMessage response = await client.PostAsJsonAsync(URL, clientData);
            TokenModel result = new TokenModel();
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadFromJsonAsync<TokenModel>();
                    result.GeneratedOn = DateTime.Now;
                }
            }
            catch
            {
                return null;
            }

            return result;
        }

        public async Task<WebUserModel> GetOwnData(string token)
        {
            WebUserModel user = null;
            string URL = Configuration.GetSection("API")["OwnDataURL"];
            using (var request = new HttpRequestMessage(HttpMethod.Get, URL))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    user = await response.Content.ReadFromJsonAsync<WebUserModel>();
                }

                return user;
            }
        }

        public async Task<WebUserModel> GetUserData(string token, string userID)
        {
            WebUserModel user = null;
            string URL = Configuration.GetSection("API")["UserDataURL"];
            URL = URL.Replace(":user", userID);
            using (var request = new HttpRequestMessage(HttpMethod.Get, URL))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    user = await response.Content.ReadFromJsonAsync<WebUserModel>();
                }

                return user;
            }
        }
    }

    public class WebUserModel
    {
        [JsonPropertyName("avatar_url")]
        public string AvatarURL { get; set; }
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
        [JsonPropertyName("default_group")]
        public string DefaultGroup { get; set; }
        [JsonPropertyName("id")]
        public int ID { get; set; }
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }
        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }
        [JsonPropertyName("is_online")]
        public bool IsOnline { get; set; }
        [JsonPropertyName("is_supporter")]
        public bool IsSupporter { get; set; }
        [JsonPropertyName("last_visit")]
        public DateTime? LastVisit { get; set; }
        [JsonPropertyName("pm_friends_only")]
        public bool PMFriendsOnly { get; set; }
        [JsonPropertyName("profile_colour")]
        public string ProfileColour { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("cover_url")]
        public string CoverURL { get; set; }
        [JsonPropertyName("discord")]
        public string Discord { get; set; }
        [JsonPropertyName("has_supported")]
        public bool HasSupported { get; set; }
        [JsonPropertyName("interests")]
        public string Interests { get; set; }
        [JsonPropertyName("join_date")]
        public DateTime JoinDate { get; set; }
        [JsonPropertyName("kudosu")]
        public WebKudosu Kudosu { get; set; }
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("max_blocks")]
        public int MaxBlocks { get; set; }
        [JsonPropertyName("max_friends")]
        public int MaxFriends { get; set; }
        [JsonPropertyName("occupation")]
        public string Occupation { get; set; }
        [JsonPropertyName("playmode")]
        public string Playmode { get; set; }
        [JsonPropertyName("playstyle")]
        public string[] Playstyle { get; set; }
        [JsonPropertyName("post_count")]
        public int PostCount { get; set; }
        [JsonPropertyName("profile_order")]
        public string[] ProfileOrder { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("title_url")]
        public string TitleURL { get; set; }
        [JsonPropertyName("twitter")]
        public string Twitter { get; set; }
        [JsonPropertyName("website")]
        public string Website { get; set; }
        [JsonPropertyName("country")]
        public WebCountry? Country { get; set; }
        [JsonPropertyName("cover")]
        public WebCover? Cover { get; set; }
        [JsonPropertyName("is_restricted")]
        public bool? IsRestricted { get; set; }

        public string IsoCountryCodeToFlagEmoji(string countryCode) => string.Concat(countryCode.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
        public string GetUserFlag()
        {
            string country = Country.Name;
            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.LCID));
            var englishRegion = regions.FirstOrDefault(region => region.EnglishName.Contains(country));
            if (englishRegion == null) return "🏳";
            var countryAbbrev = englishRegion.TwoLetterISORegionName;
            return IsoCountryCodeToFlagEmoji(countryAbbrev);
        }
    }

    public class WebCover
    {
        [JsonPropertyName("custom_url")]
        public string CustomURL { get; set; }
        [JsonPropertyName("url")]
        public string URL { get; set; }
        [JsonPropertyName("id")]
        public int? ID { get; set; }
    }

    public class WebCountry
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
    public class WebKudosu
    {
        [JsonPropertyName("available")]
        public int Available { get; set; }
        [JsonPropertyName("total")]
        public int? Total { get; set; }
    }

    public class AuthCodeGrantModel
    {
        [JsonPropertyName("client_id")]
        public int ClientID { get; set; }
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; }
        [JsonPropertyName("redirect_uri")]
        public string RedirectURI { get; set; }
    }

    public class ClientGrantModel
    {
        [JsonPropertyName("client_id")]
        public int ClientID { get; set; }
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }

    public class TokenModel
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("generated_on")]
        public DateTime? GeneratedOn { get; set; }
    }
}
