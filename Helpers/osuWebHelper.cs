using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace osb.Helpers
{
    public class OsuWebHelper
    {
        private string baseURL = "https://osu.ppy.sh/";

        void GetUser(string userID)
        {
            string siteContent = string.Empty;

            // The url you want to grab
            string url = baseURL + "/users/" + userID;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader streamReader = new StreamReader(responseStream))
            {
                siteContent = streamReader.ReadToEnd();
            }
            var webpreg = new Regex("<script id=\"json-user\" type=\"application/json\">\\s*(.+?)\\s*</script>", RegexOptions.Multiline | RegexOptions.Singleline);
            MatchCollection matches = webpreg.Matches(siteContent);
            if (matches.Count > 0)
            {
                var userRaw = matches[1].Value;
            }
            return;
        }
    }

    public class UserProfile
    {

//  avatar_url: string,
//  country_code: string,
//  default_group: string,
//  id: number,
//  is_active: boolean,
//  is_bot: boolean,
//  is_online: boolean,
//  is_supporter: boolean,
//  last_visit: string,
//  pm_friends_only: boolean,
//  profile_colour: string,
//  username: string,
//  cover_url: string,
//  discord: string,
//  has_supported: boolean,
//  interests: string,
//  join_date: string,
//  kudosu: { total: number, available: number
//    },
//  location: string,
//  max_blocks: number,
//  max_friends: number,
//  occupation: string,
//  playmode: string,
//  playstyle: string[],
//  post_count: number,
//  profile_order: string[],
//  title: string,
//  title_url: string,
//  twitter: string,
//  website: string,
//  country: { code: string, name: string
//},
//  cover: { custom_url: string, url: string, id: number},
//  is_admin: boolean,
//  is_bng: boolean,
//  is_full_bn: boolean,
//  is_gmt: boolean,
//  is_limited_bn: boolean,
//  is_moderator: boolean,
//  is_nat: boolean,
//  is_restricted: boolean,
//  is_silenced: boolean,
//  account_history:[{ description: string, id: number, length: number, timestamp: string, type: string}],
//  active_tournament_banner: string,
//  badges: [{ awarded_at: string, description: string, image_url: string, url: string }],
//  beatmap_playcounts_count: number,
//  comments_count: number,
//  favourite_beatmapset_count: number,
//  follower_count: number,
//  graveyard_beatmapset_count: number,
//  groups:[{ colour: string, has_listing: boolean, has_playmodes: boolean, id: number, identifier: string, is_probationary: boolean, name: string, short_name: string, playmodes: string }],
//  loved_beatmapset_count: number,
//  mapping_follower_count: number,
//  monthly_playcounts:[{ start_date: string, count: number }],
//  page: { html: string },
//  pending_beatmapset_count: number,
//  previous_usernames: string[],
//  ranked_beatmapset_count: number,
//  replays_watched_counts:[{ start_date: string, count: number }],
//  scores_best_count: number,
//  scores_first_count: number,
//  scores_recent_count: number,
//  statistics:
//{
//level: { current: number, progress: number },
//    global_rank: number,
//    pp: number,
//    ranked_score: number,
//    hit_accuracy: number,
//    play_count: number,
//    play_time: number,
//    total_score: number,
//    total_hits: number,
//    maximum_combo: number,
//    replays_watched_by_others: number,
//    is_ranked: boolean,
//    grade_counts: { ss: number, ssh: number, s: number, sh: number, a: number },
//    country_rank: number,
//    rank: { country: number }
//},
//  support_level: number,
//  user_achievements:[{ achieved_at: string, achievement_id: number }],
//  rankHistory: { mode: string, data: number[] },
//  rank_history: { mode: string, data: number[] },
//  ranked_and_approved_beatmapset_count: number,
//  unranked_beatmapset_count: number
    }
}
