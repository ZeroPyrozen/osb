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
                    beatmapsetHost : new MapperModel(410,"Starrodkirby86"),
                    medium: "Scripting",
                    submitDate: new DateTime(2010,12,05),
                    storyboarders: new List<StoryboarderModel>
                    {
                        new StoryboarderModel(410, "Starrodkirby86")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Classic
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1054045,
                    beatmapTitle : "Alexithymia | Lupinus | Tokei no Heya to Seishin Sekai",
                    beatmapArtist: "a_hisa",
                    beatmapsetHost : new MapperModel(3250792,"ProfessionalBox"),
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
                        StoryboardTag.Wizardry,
                        StoryboardTag.Featured
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 470977,
                    beatmapTitle : "world.execute(me);",
                    beatmapArtist: "Mili",
                    beatmapsetHost : new MapperModel(2559349,"Exile-"),
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
                        StoryboardTag.Wizardry,
                        StoryboardTag.Featured
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 173288,
                    beatmapTitle : "Kung-fu Empire",
                    beatmapArtist: "Nekomata Master+",
                    beatmapsetHost : new MapperModel(989377,"Damnae"),
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
                        StoryboardTag.Wizardry,
                        StoryboardTag.Featured
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 943052,
                    beatmapTitle : "Rise (feat. IZ*ONE)",
                    beatmapArtist: "Jonas Blue",
                    beatmapsetHost : new MapperModel(6607303, "Hokichi"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2019,03,19),
                    storyboarders: new List<StoryboarderModel>
                    {
                        new StoryboarderModel(6607303, "Hokichi"),
                        new StoryboarderModel(7405768, "TheDuckMask"),
                        new StoryboarderModel(6186628, "Affirmation")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1060207,
                    beatmapTitle : "Still Into You (Sped Up & Cut Ver.)",
                    beatmapArtist: "Paramore",
                    beatmapsetHost : new MapperModel(4230454, "Froskya"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2019,11,03),
                    storyboarders: new List<StoryboarderModel>
                    {
                        new StoryboarderModel(7798305, "Slyk"),
                        new StoryboarderModel(5420543, "PantyDev"),
                        new StoryboarderModel(5666747, "freihy"),
                        new StoryboarderModel(10198015, "BowLL"),
                        new StoryboarderModel(7405768, "TheDuckMask"),
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
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
