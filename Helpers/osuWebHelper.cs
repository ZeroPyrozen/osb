#nullable enable

using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace osb.Helpers
{
    public interface IOsuWebHelper
    {
        /// <summary>The osu! authorization page URL that starts a login. <paramref name="state"/> is echoed back to the callback.</summary>
        string GetAuthorizationUrl(string state);

        /// <summary>Exchanges an authorization code from the login callback for the user's token.</summary>
        Task<TokenModel?> GenerateAccessTokenAuthCode(string code);

        /// <summary>The logged-in user's own profile, using their token.</summary>
        Task<WebUserModel?> GetOwnData(string token);

        /// <summary>Any user's public profile, using the site's cached client token. Cached for 10 minutes.</summary>
        Task<WebUserModel?> GetUserData(int userId);
    }

    /// <summary>Calls the osu! API v2 (see https://osu.ppy.sh/docs).</summary>
    public class OsuWebHelper : IOsuWebHelper
    {
        private static readonly TimeSpan ProfileCacheTime = TimeSpan.FromMinutes(10);

        private readonly HttpClient client;
        private readonly IConfiguration Configuration;
        private readonly IMemoryCache cache;
        private readonly ILogger<OsuWebHelper> logger;

        // The HttpClient comes from IHttpClientFactory (see Program.cs), which pools and
        // recycles connections instead of leaking a socket or caching DNS forever.
        public OsuWebHelper(HttpClient client, IConfiguration configuration, IMemoryCache cache, ILogger<OsuWebHelper> logger)
        {
            this.client = client;
            Configuration = configuration;
            this.cache = cache;
            this.logger = logger;
        }

        private IConfigurationSection Api => Configuration.GetSection("API");

        public string GetAuthorizationUrl(string state)
        {
            string url = Api["AuthURL"]!
                .Replace(":client_id", Uri.EscapeDataString(Api["ClientID"] ?? ""))
                .Replace(":redirect_uri", Uri.EscapeDataString(Api["RedirectURL"] ?? ""))
                .Replace(":scope", "identify");
            return url + "&state=" + Uri.EscapeDataString(state);
        }

        public async Task<TokenModel?> GenerateAccessTokenAuthCode(string code)
        {
            var grant = new AuthCodeGrantModel
            {
                ClientID = int.Parse(Api["ClientID"]!),
                ClientSecret = Api["ClientSecret"],
                GrantType = "authorization_code",
                Code = code,
                RedirectURI = Api["RedirectURL"],
            };
            return await RequestTokenAsync(grant);
        }

        public Task<WebUserModel?> GetOwnData(string token) => GetUserAsync(Api["OwnDataURL"]!, token);

        public async Task<WebUserModel?> GetUserData(int userId)
        {
            string cacheKey = $"osu:user:{userId}";
            if (cache.TryGetValue(cacheKey, out WebUserModel? cached))
                return cached;

            string? token = await GetClientTokenAsync();
            if (token == null)
                return null;

            var user = await GetUserAsync(Api["UserDataURL"]!.Replace(":user", userId.ToString()), token);
            if (user != null)
                cache.Set(cacheKey, user, ProfileCacheTime);
            return user;
        }

        /// <summary>
        /// The site's own client-credentials token, shared by every request until shortly before it expires.
        /// Failures are remembered for a minute so a missing or wrong secret doesn't hammer the osu! API.
        /// </summary>
        private async Task<string?> GetClientTokenAsync()
        {
            const string cacheKey = "osu:client-token";
            if (cache.TryGetValue(cacheKey, out CachedToken? cached))
                return cached!.AccessToken;

            var grant = new ClientGrantModel
            {
                ClientID = int.Parse(Api["ClientID"]!),
                ClientSecret = Api["ClientSecret"],
                GrantType = "client_credentials",
                Scope = "public",
            };
            var token = await RequestTokenAsync(grant);

            if (token?.AccessToken == null)
            {
                logger.LogWarning("Couldn't get an osu! API client token. Check API__ClientID and API__ClientSecret.");
                cache.Set(cacheKey, new CachedToken(null), TimeSpan.FromMinutes(1));
                return null;
            }

            cache.Set(cacheKey, new CachedToken(token.AccessToken), TimeSpan.FromSeconds(Math.Max(60, token.ExpiresIn - 300)));
            return token.AccessToken;
        }

        private async Task<TokenModel?> RequestTokenAsync(object grant)
        {
            try
            {
                var response = await client.PostAsJsonAsync(Api["TokenURL"], grant);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TokenModel>() : null;
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                logger.LogWarning(e, "osu! token request failed.");
                return null;
            }
        }

        private async Task<WebUserModel?> GetUserAsync(string url, string token)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var response = await client.SendAsync(request);
                return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<WebUserModel>() : null;
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                logger.LogWarning(e, "osu! user request failed for {Url}.", url);
                return null;
            }
        }

        private sealed record CachedToken(string? AccessToken);
    }

    // JSON shapes of the osu! API responses used by the site.
#nullable disable
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
        public WebCountry Country { get; set; }
        [JsonPropertyName("cover")]
        public WebCover Cover { get; set; }
        [JsonPropertyName("is_restricted")]
        public bool? IsRestricted { get; set; }
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
