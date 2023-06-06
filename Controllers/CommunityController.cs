using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;

namespace osb.Controllers
{
    public class CommunityController : BaseController
    {
        private IConfiguration _configuration;
        private IOsuWebHelper _osuWebHelper;

        public CommunityController(IConfiguration iconfig, IOsuWebHelper osuWebHelper)
        {
            _configuration = iconfig;
            _osuWebHelper = osuWebHelper;
        }

        public IActionResult Index()
        {
            CommunityViewModel communityViewModel = new CommunityViewModel();
            //communityViewModel.storyboarders = new osbDatabaseHelper(_configuration).GetCommunityStoryboarder();
            communityViewModel.storyboarders = DummyHelper.GenerateStoryboarders()
                .OrderByDescending(x => x.GetPrimaryRole()?.RoleID)
                .ThenBy(y => y.Username)
                .ToList();
            //if (HttpContext.Session.GetString(SessionEnum.UserData) != null)
            //{
            //    communityViewModel.testValue = HttpContext.Session.GetString(SessionEnum.UserData);
            //}
            return View("Index", communityViewModel);
        }

        public async Task<IActionResult> Profile(int userID)
        {
            if (userID == 0)
                return NotFound();
            CommunityViewModel communityViewModel = new CommunityViewModel();
            //Verify Session Client Credential Exists
            bool isTokenAvailable = IsClientTokenAvailable();

            try
            {
                if (!isTokenAvailable)
                {
                    TokenModel token = await _osuWebHelper.GenerateAccessTokenClient();
                    HttpContext.Session.SetString(SessionEnum.ClientToken, JsonConvert.SerializeObject(token));
                }

                communityViewModel.webUserData =
                    await _osuWebHelper.GetUserData(GetClientToken().AccessToken, userID.ToString());
            }
            catch (Exception e)
            {
                return NotFound();
            }

            if (communityViewModel.webUserData == null)
            {
                return NotFound();
            }

            //TODO: Dummy Way - Replace this with Database Access
            communityViewModel.storyboarder = DummyHelper.GenerateStoryboarder(userID);
            if (communityViewModel.storyboarder == null)
            {
                //Storyboarder hasn't joined osb Discord Server
                communityViewModel.storyboarder = DummyHelper.GetStoryboarderFromBeatmaps(userID);
                if (communityViewModel.storyboarder == null)
                {
                    if (communityViewModel.webUserData == null)
                    {
                        return NotFound();
                    }

                    communityViewModel.storyboarder = new StoryboarderModel(userID,
                        communityViewModel.webUserData.Username, string.Empty,
                        new List<DiscordRoleModel> { DiscordRoleModel.Verified });
                }
                else
                {
                    // communityViewModel.storyboarder.Roles = new List<DiscordRoleModel>
                    //     { DiscordRoleModel.Storyboarder };
                    // commented by @mysterymarshak because:
                    // 1. useless
                    // 2. overrides roles enumerable
                }
            }

            communityViewModel.storyboarderBeatmaps = DummyHelper.GenerateBeatmapsByStoryboarder(userID);
            communityViewModel.baseURL = "https://" + Request.Host;

            return View("Profile", communityViewModel);
        }
    }
}