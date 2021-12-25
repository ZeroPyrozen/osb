using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace osb.Models
{
    public class BeatmapModel
    {
        public int BeatmapsetID { get; set; }
        public string BeatmapTitle { get; set; }
        public string BeatmapArtist { get; set; }
        public MapperModel BeatmapsetHost { get; set; }
        public string Medium { get; set; }
        public DateTime SubmitDate { get; set; }
        public string BeatmapCardURL { get; set; }
        public string BeatmapListURL { get; set; }
        public string BeatmapCoverURL { get; set; }
        public string BeatmapPageURL { get; set; }
        public List<StoryboarderModel> Storyboarders { get; set; }
        public List<StoryboardTag> StoryboardTags { get; set; }
        //TODO: Add Video URL, Screenshots, and Storyboarder Comments in Separate Classes to reduce memory load

        public BeatmapModel(int beatmapsetID, string beatmapTitle, string beatmapArtist, MapperModel beatmapsetHost, string medium, DateTime submitDate, List<StoryboarderModel> storyboarders, List<StoryboardTag> tags)
        {
            this.BeatmapsetID = beatmapsetID;
            this.BeatmapTitle = beatmapTitle;
            this.BeatmapArtist = beatmapArtist;
            this.BeatmapsetHost = beatmapsetHost;
            this.Medium = medium;
            this.SubmitDate = submitDate;
            this.Storyboarders = storyboarders;
            this.StoryboardTags = tags;
            //TODO: I will make it more dynamic by moving it to appsettings.json
            this.BeatmapCardURL = "https://assets.ppy.sh/beatmaps/" + beatmapsetID.ToString() + "/covers/card.jpg";
            this.BeatmapCoverURL = "https://assets.ppy.sh/beatmaps/" + beatmapsetID.ToString() + "/covers/cover.jpg";
            this.BeatmapListURL = "https://assets.ppy.sh/beatmaps/" + beatmapsetID.ToString() + "/covers/list.jpg";
            this.BeatmapPageURL = "https://osu.ppy.sh/beatmapsets/" + beatmapsetID.ToString();
        }

        public int GetBeatmapsetIDByStoryboarders(string searchQuery)
        {
            if (this.Storyboarders.Any(x => x.Username.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
                return BeatmapsetID;
            else
                return -1;
        }

        public int GetBeatmapsetIDByMappers(string searchQuery)
        {
            if (this.BeatmapsetHost.Username.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                return BeatmapsetID;
            else
                return -1;
        }

        public int GetBeatmapsetIDByTags(string searchQuery)
        {
            if (this.StoryboardTags.Any(x => x.TagName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
                return BeatmapsetID;
            else
                return -1;
        }

        public int GetStoryboardRating()
        {
            int rating = 0;
            for(int i=0; i<StoryboardTags.Count; i++)
            {
                rating += StoryboardTags[i].Rating;
            }
            return rating;
        }
    }

    public class StoryboardTag
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public string EscapedTagName { get; set; }
        public int Rating { get; set; }

        public static StoryboardTag Wizardry = new StoryboardTag(1,"Wizardry",30);
        public static StoryboardTag Pixels = new StoryboardTag(2,"Pixels",20);
        public static StoryboardTag Rave = new StoryboardTag(3,"Rave",10);
        public static StoryboardTag Lyrics = new StoryboardTag(4,"Lyrics",10);
        public static StoryboardTag ThreeD = new StoryboardTag(5,"3D",30);
        public static StoryboardTag Narrative = new StoryboardTag(6,"Narrative",30);
        public static StoryboardTag AudioVisualization = new StoryboardTag(7,"Audio Visualization",15);
        public static StoryboardTag Particles = new StoryboardTag(8,"Particles",15);
        public static StoryboardTag FullControl = new StoryboardTag(9,"Full Control",20);
        public static StoryboardTag GameplayEnhancement = new StoryboardTag(10,"Gameplay Enhancement",15);
        public static StoryboardTag Featured = new StoryboardTag(11, "Featured", 20);
        public static StoryboardTag Classic = new StoryboardTag(12, "Classic", 10);

        public StoryboardTag(int tagID, string tagName, int rating)
        {
            this.TagID = tagID;
            this.TagName = tagName;
            this.Rating = rating;
            this.EscapedTagName = tagName.Replace(" ", "_").ToLower();
        }
    }
}
