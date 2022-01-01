using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;

namespace osb.Controllers
{
    public class ShowcaseController : Controller
    {
        public IActionResult Index()
        {
            ShowcaseViewModel showcaseViewModel = new ShowcaseViewModel();
            //Populate with dummy data
            showcaseViewModel.searchQuery = "";
            showcaseViewModel.beatmaps = DummyHelper.GenerateBeatmaps().OrderByDescending(x=>x.ShowcasedDate).ToList();
            showcaseViewModel.baseURL = "https://" + this.Request.Host;
            return View("Index", showcaseViewModel);
        }

        public IActionResult Search(string s, string f, string c, string t)
        {
            ShowcaseViewModel showcaseViewModel = new ShowcaseViewModel();
            showcaseViewModel.baseURL = "https://"+ this.Request.Host;
            showcaseViewModel.searchQuery = (s != null) ? s : "";
            showcaseViewModel.beatmaps = DummyHelper.GenerateBeatmaps();
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
                t = t.Replace('_', ' ');
                showcaseViewModel.beatmaps = showcaseViewModel.beatmaps.Where
                (x =>
                    (
                        x.BeatmapsetID == x.GetBeatmapsetIDByTags(t)
                    )
                ).ToList();
                showcaseViewModel.searchQuery = t;
            }
            if(showcaseViewModel.beatmaps.Count > 0)
            {
                showcaseViewModel.beatmaps = showcaseViewModel.beatmaps.OrderByDescending(x => x.ShowcasedDate).ToList();
            }
            return View("Index", showcaseViewModel);
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
    }
}
