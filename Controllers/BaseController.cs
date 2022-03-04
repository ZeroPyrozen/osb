using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;

namespace osb.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if(HttpContext.Session.Get("Login") != null)
            {
                ViewData["Login"] = HttpContext.Session.Get("Login");
                WebUserModel user = JsonConvert.DeserializeObject<WebUserModel>(HttpContext.Session.GetString(SessionEnum.UserData));
                
                ViewData["Username"] = user.Username;
                ViewData["UserID"] = user.ID;
            }
                
            base.OnActionExecuting(context);
        }

        public TokenModel GetClientToken()
        {
            TokenModel clientToken = JsonConvert.DeserializeObject<TokenModel>(HttpContext.Session.GetString(SessionEnum.ClientToken));
            return clientToken;
        }

        public TokenModel GetAuthCodeToken()
        {
            TokenModel authCodeToken = JsonConvert.DeserializeObject<TokenModel>(HttpContext.Session.GetString(SessionEnum.AuthCodeToken));
            return authCodeToken;
        }

        public bool IsClientTokenAvailable()
        {
            if (HttpContext.Session.GetString(SessionEnum.ClientToken) == null)
                return false;
            TokenModel clientToken = GetClientToken();
            if (clientToken.GeneratedOn.Value.AddSeconds(clientToken.ExpiresIn - 1) < DateTime.Now)
                return false;
            return true;
        }

        public bool IsAuthCodeTokenAvailable()
        {
            if (HttpContext.Session.GetString(SessionEnum.AuthCodeToken) == null)
                return false;
            TokenModel authCodeToken = GetAuthCodeToken();
            if (authCodeToken.GeneratedOn.Value.AddSeconds(authCodeToken.ExpiresIn - 1) < DateTime.Now)
                return false;
            return true;
        }
    }
}
