using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace osb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            HomeViewModel homeViewModel = new HomeViewModel();
            homeViewModel.recentBeatmaps = DummyHelper.GenerateBeatmaps();
            homeViewModel.totalStoryboard = homeViewModel.recentBeatmaps.Count;
            homeViewModel.totalStoryboarder = GetStoryboarderCount(homeViewModel.recentBeatmaps);
            homeViewModel.mediumFrequency = GetStoryboardMediumFrequency(homeViewModel.recentBeatmaps);
            homeViewModel.randomBeatmap = GetRandomBeatmap(homeViewModel.recentBeatmaps);
            homeViewModel.recentBeatmaps = homeViewModel.recentBeatmaps.OrderByDescending(x => x.ShowcasedDate).Take(5).ToList();
            homeViewModel.baseURL = "https://" + this.Request.Host;
            return View("Index", homeViewModel);
        }

        private BeatmapModel GetRandomBeatmap(List<BeatmapModel> beatmaps)
        {
            if(beatmaps != null & beatmaps.Count > 0)
            {
                List<BeatmapModel> beatmapHasVideo = beatmaps.Where(x => string.IsNullOrEmpty(x.VideoURL) == false).ToList();
                var random = new Random();
                int index = random.Next(beatmapHasVideo.Count);
                return beatmapHasVideo[index];
            }
            return null;
        }

        private int GetStoryboarderCount(List<BeatmapModel> beatmaps)
        {
            List<StoryboarderModel> countedStoryboarder = new List<StoryboarderModel>();
            for(int i=0; i<beatmaps.Count; i++)
            {
                for(int j=0; j<beatmaps[i].Storyboarders.Count; j++)
                {
                    if(!countedStoryboarder.Any(x=>x.UserID == beatmaps[i].Storyboarders[j].UserID))
                    {
                        countedStoryboarder.Add(beatmaps[i].Storyboarders[j]);
                    }
                }
            }
            return countedStoryboarder.Count;
        }

        private List<KeyValuePair<string, int>> GetStoryboardMediumFrequency(List<BeatmapModel> beatmaps)
        {
            var distinctValues = beatmaps
                                  .GroupBy(p => p.Medium)
                                  .Select(g => g.First())
                                  .ToList();
            List<KeyValuePair<string, int>> mediumFrequency = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < distinctValues.Count; i++)
            {
                var count = beatmaps.Count(e => e.Medium == distinctValues[i].Medium);
                mediumFrequency.Add(new KeyValuePair<string, int>(distinctValues[i].Medium,count));
            }
            return mediumFrequency.OrderByDescending(x=>x.Value).ToList();
        }
    }
}
