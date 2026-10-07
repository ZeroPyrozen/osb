#nullable enable

using System.Net;
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

        /// <summary>A beatmapset, by its ID.</summary>
        Task<OsuLookup<WebBeatmapsetModel>> GetBeatmapsetData(int beatmapsetId);

        /// <summary>A beatmap (one difficulty), by its ID. Its beatmapset ID resolves difficulty links.</summary>
        Task<OsuLookup<WebBeatmapModel>> GetBeatmapData(int beatmapId);

        /// <summary>
        /// An account, by user ID or username. osu! reads a number as an ID first, and also finds people
        /// by a previous username. Accounts that are found are cached for 10 minutes.
        /// </summary>
        Task<OsuLookup<WebUserModel>> FindUser(string idOrUsername);
    }

    /// <summary>
    /// The answer to an osu! API lookup: what was found, or a null <see cref="Value"/> when osu! has no
    /// such thing. <see cref="Unavailable"/> means osu! couldn't be asked or didn't answer properly (no
    /// client secret, network trouble, an error), so callers can say "try again later", not "not found".
    /// </summary>
    public sealed record OsuLookup<T>(T? Value, bool Unavailable = false) where T : class
    {
        // Named, because new(null) would call the record's copy constructor.
        public static OsuLookup<T> NotFound { get; } = new(Value: null);

        public static OsuLookup<T> Failed { get; } = new(null, Unavailable: true);
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

        public async Task<WebUserModel?> GetOwnData(string token) =>
            (await GetAsync<WebUserModel>(Api["OwnDataURL"]!, token)).Value;

        public async Task<WebUserModel?> GetUserData(int userId)
        {
            string cacheKey = $"osu:user:{userId}";
            if (cache.TryGetValue(cacheKey, out WebUserModel? cached))
                return cached;

            string? token = await GetClientTokenAsync();
            if (token == null)
                return null;

            var user = (await GetAsync<WebUserModel>(Api["UserDataURL"]!.Replace(":user", userId.ToString()), token)).Value;
            if (user != null)
                cache.Set(cacheKey, user, ProfileCacheTime);
            return user;
        }

        public Task<OsuLookup<WebBeatmapsetModel>> GetBeatmapsetData(int beatmapsetId) =>
            LookupAsync<WebBeatmapsetModel>(Api["BeatmapsetDataURL"]!.Replace(":beatmapset", beatmapsetId.ToString()));

        public Task<OsuLookup<WebBeatmapModel>> GetBeatmapData(int beatmapId) =>
            LookupAsync<WebBeatmapModel>(Api["BeatmapDataURL"]!.Replace(":beatmap", beatmapId.ToString()));

        public async Task<OsuLookup<WebUserModel>> FindUser(string idOrUsername)
        {
            string name = idOrUsername.Trim();
            if (name.Length == 0)
                return OsuLookup<WebUserModel>.NotFound;

            string cacheKey = $"osu:find:{name.ToLowerInvariant()}";
            if (cache.TryGetValue(cacheKey, out WebUserModel? cached))
                return new OsuLookup<WebUserModel>(cached);

            // A plain number is looked up as an ID, then as a username; "@name" only as a username.
            string user = name.All(char.IsAsciiDigit) ? name : "@" + Uri.EscapeDataString(name);
            var found = await LookupAsync<WebUserModel>(Api["UserDataURL"]!.Replace(":user", user));
            if (found.Value != null)
                cache.Set(cacheKey, found.Value, ProfileCacheTime);
            return found;
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

        /// <summary>A lookup with the site's own client token.</summary>
        private async Task<OsuLookup<T>> LookupAsync<T>(string url) where T : class =>
            await GetClientTokenAsync() is { } token ? await GetAsync<T>(url, token) : OsuLookup<T>.Failed;

        private async Task<OsuLookup<T>> GetAsync<T>(string url, string token) where T : class
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var response = await client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return OsuLookup<T>.NotFound;
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("osu! answered {Status} for {Url}.", (int)response.StatusCode, url);
                    return OsuLookup<T>.Failed;
                }
                return await response.Content.ReadFromJsonAsync<T>() is { } value ? new OsuLookup<T>(value) : OsuLookup<T>.Failed;
            }
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                logger.LogWarning(e, "osu! request failed for {Url}.", url);
                return OsuLookup<T>.Failed;
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

    public class WebBeatmapsetModel
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("artist")]
        public string Artist { get; set; }
        [JsonPropertyName("creator")]
        public string Creator { get; set; }
        [JsonPropertyName("user_id")]
        public int UserID { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("storyboard")]
        public bool Storyboard { get; set; }
        [JsonPropertyName("video")]
        public bool Video { get; set; }
        [JsonPropertyName("submitted_date")]
        public DateTimeOffset? SubmittedDate { get; set; }
        [JsonPropertyName("ranked_date")]
        public DateTimeOffset? RankedDate { get; set; }
    }

    public class WebBeatmapModel
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }
        [JsonPropertyName("beatmapset_id")]
        public int BeatmapsetID { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
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
