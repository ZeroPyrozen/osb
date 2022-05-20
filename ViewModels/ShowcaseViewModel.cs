using osb.Models;
using System;
using System.Collections.Generic;

namespace osb.ViewModels
{
    //Test
    public class ShowcaseViewModel
    {
        public BeatmapModel beatmapDetail { get; set; }
        public List<BeatmapModel> beatmaps { get; set; }
        public string baseURL { get; set; }
        public string searchQuery { get; set; }
    }
}
