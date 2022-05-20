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
    public class AuthController : BaseController
    {
        private IConfiguration _configuration;
        private OsuWebHelper _osuWebHelper;
        public AuthController(IConfiguration iconfig)
        {
            _configuration = iconfig;
            _osuWebHelper = new OsuWebHelper(_configuration);
        }
        public IActionResult Index()
        {
            string redirectURL = _osuWebHelper.GetAuthorizationCode();
            return Redirect(redirectURL);
        }

        public async Task<IActionResult> Authorized(string code, string error)
        {
            if(!string.IsNullOrEmpty(error))
                return RedirectToAction("Index", "Home");
            HttpContext.Session.SetString("Login", "Test");
            TokenModel token = await _osuWebHelper.GenerateAccessTokenAuthCode(code);
            WebUserModel user = await _osuWebHelper.GetOwnData(token.AccessToken);

            HttpContext.Session.SetString(SessionEnum.AuthCodeToken, JsonConvert.SerializeObject(token));
            HttpContext.Session.SetString(SessionEnum.UserData, JsonConvert.SerializeObject(user));

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Login");

            return RedirectToAction("Index", "Home");
        }
    }
}
