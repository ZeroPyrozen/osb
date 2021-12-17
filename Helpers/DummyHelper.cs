using osb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace osb.Helpers
{
    public class DummyHelper
    {
        public static List<StoryboarderModel>GenerateStoryboarders()
        {
            return new List<StoryboarderModel>
            {
                new StoryboarderModel
                (
                    989377,
                    "Damnae",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Mentor,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    3482692,
                    "WildOne94",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Moderator,
                        DiscordRoleModel.Sidetail,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    3664366,
                    "-Tochi",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    2559349,
                    "Exile-",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    410,
                    "Starrodkirby86",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    }
                ),
                new StoryboarderModel
                (
                    3031177,
                    "Mamat",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    4422807,
                    "Erasome",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    3035836,
                    "spaceman_atlas",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Verified
                    }
                )
            };
        }
        public static List<BeatmapModel> GenerateBeatmaps()
        {
            List<BeatmapModel> beatmaps = new List<BeatmapModel>
            {
                new BeatmapModel
                (
                    beatmapsetID: 16457,
                    beatmapTitle : "SWITCHED-ON LOTUS",
                    beatmapArtist: "Susumu Hirasawa",
                    beatmapsetHost : "Starrodkirby86",
                    medium: "Scripting",
                    submitDate: new DateTime(2010,12,05),
                    storyboarders: new List<StoryboarderModel>
                    {
                        new StoryboarderModel(410, "Starrodkirby86")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Lyrics
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1054045,
                    beatmapTitle : "Alexithymia | Lupinus | Tokei no Heya to Seishin Sekai",
                    beatmapArtist: "a_hisa",
                    beatmapsetHost : "ProfessionalBox",
                    medium: "Storybrew",
                    submitDate: new DateTime(2019,10,20),
                    storyboarders: new List<StoryboarderModel>
                    {
                        new StoryboarderModel(3664366, "-Tochi"),
                        new StoryboarderModel(4610047, "PoNo")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Wizardry
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 470977,
                    beatmapTitle : "world.execute(me);",
                    beatmapArtist: "Mili",
                    beatmapsetHost : "Exile-",
                    medium: "Java",
                    submitDate: new DateTime(2017,01,29),
                    storyboarders: new List<StoryboarderModel>
                    {
                        new StoryboarderModel(2559349, "Exile-")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Narrative,
                        StoryboardTag.ThreeD,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Pixels,
                        StoryboardTag.Wizardry
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 173288,
                    beatmapTitle : "Kung-fu Empire",
                    beatmapArtist: "Nekomata Master+",
                    beatmapsetHost : "Damnae",
                    medium: "C#",
                    submitDate: new DateTime(2015,05,24),
                    storyboarders: new List<StoryboarderModel>
                    {
                        new StoryboarderModel(989377, "Damnae")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Pixels,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Wizardry
                    }
                )
            };
            return beatmaps;
        }
        public static BeatmapModel GenerateBeatmap(int beatmapsetID)
        {
            BeatmapModel beatmap = GenerateBeatmaps().Where(x => x.BeatmapsetID == beatmapsetID).FirstOrDefault();
            return beatmap;
        }
    }
}
