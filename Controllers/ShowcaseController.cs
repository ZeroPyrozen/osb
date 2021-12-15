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
            CommunityViewModel communityViewModel = new CommunityViewModel();
            //Populate with dummy data
            communityViewModel.storyboarders = DummyHelper.GenerateStoryboarders();

            return View("Index");
        }

        public IActionResult Search(string s, string f, string c)
        {
            CommunityViewModel communityViewModel = new CommunityViewModel();
            //Populate with dummy data
            communityViewModel.storyboarders = DummyHelper.GenerateStoryboarders();

            return View("Index");
        }
    }
}
