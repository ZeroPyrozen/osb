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
    }

    public class StoryboardTag
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public string EscapedTagName { get; set; }

        public static StoryboardTag Wizardry = new StoryboardTag(1,"Wizardry");
        public static StoryboardTag Pixels = new StoryboardTag(2,"Pixels");
        public static StoryboardTag Rave = new StoryboardTag(3,"Rave");
        public static StoryboardTag Lyrics = new StoryboardTag(4,"Lyrics");
        public static StoryboardTag ThreeD = new StoryboardTag(5,"3D");
        public static StoryboardTag Narrative = new StoryboardTag(6,"Narrative");
        public static StoryboardTag AudioVisualization = new StoryboardTag(7,"Audio Visualization");
        public static StoryboardTag Particles = new StoryboardTag(8,"Particles");
        public static StoryboardTag FullControl = new StoryboardTag(9,"Full Control");
        public static StoryboardTag GameplayEnhancement = new StoryboardTag(10,"Gameplay Enhancement");

        public StoryboardTag(int tagID, string tagName)
        {
            this.TagID = tagID;
            this.TagName = tagName;
            this.EscapedTagName = tagName.Replace(" ", "_").ToLower();
        }
    }
}
