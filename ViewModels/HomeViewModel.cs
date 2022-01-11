using osb.Models;
using System;
using System.Collections.Generic;

namespace osb.ViewModels
{
    public class HomeViewModel
    {
        public List<BeatmapModel> recentBeatmaps { get; set; }
        public string baseURL { get; set; }
        public int totalStoryboard { get; set; }
        public int totalStoryboarder { get; set; }
        public BeatmapModel randomBeatmap { get; set; }
        public List<KeyValuePair<string, int>> mediumFrequency { get; set; }
    }
}
