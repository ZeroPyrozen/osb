using osb.Helpers;
using osb.Models;
using System;
using System.Collections.Generic;

namespace osb.ViewModels
{
    public class CommunityViewModel
    {
        public List<StoryboarderModel> storyboarders;
        public WebUserModel webUserData;
        public StoryboarderModel storyboarder;
        public List<BeatmapModel> storyboarderBeatmaps;
        public string baseURL { get; set; }
        public string testValue { get; set; }
    }
}
