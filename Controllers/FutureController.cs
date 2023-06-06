using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;

namespace osb.Controllers
{
    public class FutureController : BaseController
    {
        private readonly IOsuWebHelper _osuWebHelper;

        public FutureController(IOsuWebHelper osuWebHelper)
        {
            _osuWebHelper = osuWebHelper;
        }

        public IActionResult Index(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
                return View();
            return Redirect("Home");
        }

        private BeatmapModel GetRandomBeatmap(List<BeatmapModel> beatmaps)
        {
            if (beatmaps != null & beatmaps.Count > 0)
            {
                List<BeatmapModel> beatmapHasVideo = beatmaps
                    .Where(x => string.IsNullOrEmpty(x.VideoURL) == false)
                    .ToList();
                var random = new Random();
                int index = random.Next(beatmapHasVideo.Count);
                return beatmapHasVideo[index];
            }

            return null;
        }

        private int GetStoryboarderCount(List<BeatmapModel> beatmaps)
        {
            List<StoryboarderModel> countedStoryboarder = new List<StoryboarderModel>();
            for (int i = 0; i < beatmaps.Count; i++)
            {
                for (int j = 0; j < beatmaps[i].Storyboarders.Count; j++)
                {
                    if (
                        !countedStoryboarder.Any(
                            x => x.UserID == beatmaps[i].Storyboarders[j].UserID
                        )
                    )
                    {
                        countedStoryboarder.Add(beatmaps[i].Storyboarders[j]);
                    }
                }
            }

            return countedStoryboarder.Count;
        }

        private List<KeyValuePair<string, int>> GetStoryboardMediumFrequency(
            List<BeatmapModel> beatmaps
        )
        {
            var distinctValues = beatmaps.GroupBy(p => p.Medium).Select(g => g.First()).ToList();
            List<KeyValuePair<string, int>> mediumFrequency = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < distinctValues.Count; i++)
            {
                var count = beatmaps.Count(e => e.Medium == distinctValues[i].Medium);
                mediumFrequency.Add(new KeyValuePair<string, int>(distinctValues[i].Medium, count));
            }

            return mediumFrequency.OrderByDescending(x => x.Value).ToList();
        }

        public IActionResult Home(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
            {
                HomeViewModel homeViewModel = new HomeViewModel();
                homeViewModel.recentBeatmaps = DummyHelper.GenerateBeatmaps();
                homeViewModel.totalStoryboard = homeViewModel.recentBeatmaps.Count;
                homeViewModel.totalStoryboarder = GetStoryboarderCount(
                    homeViewModel.recentBeatmaps
                );
                homeViewModel.mediumFrequency = GetStoryboardMediumFrequency(
                    homeViewModel.recentBeatmaps
                );
                homeViewModel.randomBeatmap = GetRandomBeatmap(homeViewModel.recentBeatmaps);
                homeViewModel.recentBeatmaps = homeViewModel.recentBeatmaps
                    .OrderByDescending(x => x.ShowcasedDate)
                    .Take(5)
                    .ToList();
                homeViewModel.baseURL = "https://" + this.Request.Host;
                return View(homeViewModel);
            }

            return Redirect("Home");
        }

        public IActionResult Community(string code)
        {
            if (code != "WYSI")
            {
                return Redirect("Home");
            }

            var communityViewModel = new CommunityViewModel();

            var storyBoarders = DummyHelper.GenerateStoryboarders()
                .OrderByDescending(x => x.GetPrimaryRole()?.RoleID)
                .ThenBy(y => y.Username)
                .ToList();

            communityViewModel.storyboarders = storyBoarders;

            return View(communityViewModel);
        }

        public IActionResult Showcase(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
            {
                ShowcaseViewModel showcaseViewModel = new ShowcaseViewModel();
                //Populate with dummy data
                showcaseViewModel.searchQuery = "";
                showcaseViewModel.beatmaps = DummyHelper
                    .GenerateBeatmaps()
                    .OrderByDescending(x => x.ShowcasedDate)
                    .ToList();
                showcaseViewModel.storyboardTags = StoryboardTag.GetAllStoryboardTags();
                showcaseViewModel.baseURL = "https://" + this.Request.Host;
                return View(showcaseViewModel);
            }

            return Redirect("Home");
        }

        public IActionResult Search(string s, string f, string c, string t)
        {
            ShowcaseViewModel showcaseViewModel = new ShowcaseViewModel();
            showcaseViewModel.baseURL = "https://" + this.Request.Host;
            showcaseViewModel.searchQuery = (s != null) ? s : "";
            showcaseViewModel.beatmaps = DummyHelper.GenerateBeatmaps();
            showcaseViewModel.storyboardTags = StoryboardTag.GetAllStoryboardTags();
            StringComparison filterRule = StringComparison.OrdinalIgnoreCase;
            if (s != null)
            {
                showcaseViewModel.beatmaps = showcaseViewModel.beatmaps.Where
                (x =>
                    (
                        x.BeatmapArtist.Contains(s, filterRule) ||
                        x.BeatmapTitle.Contains(s, filterRule) ||
                        x.BeatmapsetID == x.GetBeatmapsetIDByMappers(s) ||
                        x.BeatmapsetID == x.GetBeatmapsetIDByStoryboarders(s)
                    )
                ).ToList();
            }

            if (t != null)
            {
                int tagID = -1;

                if (t != "-1" && int.TryParse(t, out tagID))
                {
                    t = t.Replace('_', ' ');
                    showcaseViewModel.beatmaps = showcaseViewModel.beatmaps.Where
                    (x =>
                        (
                            x.BeatmapsetID == x.GetBeatmapsetIDByTags(t)
                        )
                    ).ToList();
                    showcaseViewModel.searchTag = t;
                }
            }

            if (showcaseViewModel.beatmaps.Count > 0)
            {
                showcaseViewModel.beatmaps =
                    showcaseViewModel.beatmaps.OrderByDescending(x => x.ShowcasedDate).ToList();
            }

            return View("Showcase", showcaseViewModel);
        }

        public IActionResult Detail(int beatmapsetID)
        {
            ShowcaseViewModel showcaseViewModel = new ShowcaseViewModel();
            if (beatmapsetID == 0)
                return NotFound();
            //Populate with dummy data
            showcaseViewModel.beatmapDetail = DummyHelper.GenerateBeatmap(beatmapsetID);
            showcaseViewModel.baseURL = "https://" + this.Request.Host;
            if (showcaseViewModel.beatmapDetail == null)
            {
                return NotFound();
            }

            return View("Detail", showcaseViewModel);
        }

        public IActionResult NotFound(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
                return View();
            return Redirect("Home");
        }

        public IActionResult Learn(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
                return View();
            return Redirect("Home");
        }

        public IActionResult Profile(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
                return View();
            return Redirect("Home");
        }
    }
}