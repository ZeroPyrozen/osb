using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;
using Westwind.AspNetCore.Markdown;

namespace osb.Controllers
{
    public class LearnController : Controller
    {
        private IConfiguration _configuration;
        public LearnController(IConfiguration iconfig)
        {
            _configuration = iconfig;
        }
        public IActionResult Index()
        {
            LearnViewModel learnViewModel = new LearnViewModel();
            //TODO: Implement template page so it won't require actionresult for new page
            //TODO: Implement breadcrumb for easy access
            var parsedHtml = Markdown.ParseFromUrl("https://gist.github.com/ZeroPyrozen/96ac0da10c741f4679fc0cb51c12a439");
            learnViewModel.indexTest = parsedHtml;
            return View("Index", learnViewModel);
        }

    }
}
