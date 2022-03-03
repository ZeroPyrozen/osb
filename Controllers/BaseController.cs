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
