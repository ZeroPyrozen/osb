using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;

namespace osb.Controllers
{
    public class CommunityController : Controller
    {
        private IConfiguration _configuration;
        public CommunityController(IConfiguration iconfig)
        {
            _configuration = iconfig;
        }
        public IActionResult Index()
        {
            CommunityViewModel communityViewModel = new CommunityViewModel();
            communityViewModel.storyboarders = new osbDatabaseHelper(_configuration).GetCommunityStoryboarder();
            return View("Index", communityViewModel);
        }
    }
}
