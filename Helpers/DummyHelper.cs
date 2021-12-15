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
                    "989377",
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
                    "3482692",
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
                    "3664366",
                    "-Tochi",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    "2559349",
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
                    "410",
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
                    "3031177",
                    "Mamat",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    "4422807",
                    "Erasome",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    }
                ),
                new StoryboarderModel
                (
                    "3035836",
                    "spaceman_atlas",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Verified
                    }
                )
            };
        }
    }
}
