using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;

namespace osb.Controllers
{
    public class CommunityController : BaseController
    {
        private IConfiguration _configuration;
        private OsuWebHelper _osuWebHelper;
        public CommunityController(IConfiguration iconfig)
        {
            _configuration = iconfig;
            _osuWebHelper = new OsuWebHelper(_configuration);
        }
        public IActionResult Index()
        {
            CommunityViewModel communityViewModel = new CommunityViewModel();
            //communityViewModel.storyboarders = new osbDatabaseHelper(_configuration).GetCommunityStoryboarder();
            communityViewModel.storyboarders = DummyHelper.GenerateStoryboarders().OrderByDescending(x => x.GetPrimaryRole().RoleID).ThenBy(y => y.Username).ToList();
            //if (HttpContext.Session.GetString(SessionEnum.UserData) != null)
            //{
            //    communityViewModel.testValue = HttpContext.Session.GetString(SessionEnum.UserData);
            //}
            return View("Index", communityViewModel);
        }

        public IActionResult Profile(int userID)
        {
            if (userID == 0)
                return NotFound();
            CommunityViewModel communityViewModel = new CommunityViewModel();
            communityViewModel.storyboarder = DummyHelper.GenerateStoryboarder(userID);
            if(communityViewModel.storyboarder == null)
            {
                //Storyboarder hasn't joined osb Discord Server
                communityViewModel.storyboarder = DummyHelper.GetStoryboarderFromBeatmaps(userID);
                if (communityViewModel.storyboarder == null)
                    return NotFound();
                else
                {
                    communityViewModel.storyboarder.Roles = new List<DiscordRoleModel>();
                    communityViewModel.storyboarder.Roles.Add(DiscordRoleModel.Storyboarder);
                }    
            }
            communityViewModel.storyboarderBeatmaps = DummyHelper.GenerateBeatmapsByStoryboarder(userID);
            communityViewModel.baseURL = "https://" + this.Request.Host;
            return View("Profile", communityViewModel);
        }

        //public IActionResult Login()
        //{
        //    string redirectURL = _osuWebHelper.GetAuthorizationCode();
        //    return Redirect(redirectURL);
        //}

        //public IActionResult Authorized(string code)
        //{
        //    HttpContext.Session.SetString(SessionEnum.UserData,"Test");
        //    return RedirectToAction("Index");
        //}
    }
}
