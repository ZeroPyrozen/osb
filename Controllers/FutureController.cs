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
    public class FutureController : BaseController
    {
        private IConfiguration _configuration;
        public FutureController(IConfiguration iconfig)
        {
            _configuration = iconfig;
        }
        public IActionResult Index(string code)
        {
            if(!string.IsNullOrEmpty(code) && code == "WYSI")
                return View();
            return Redirect("Home");
        }

        public IActionResult Home(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
                return View();
            return Redirect("Home");
        }

        public IActionResult Community(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
                return View();
            return Redirect("Home");
        }

        public IActionResult Showcase(string code)
        {
            if (!string.IsNullOrEmpty(code) && code == "WYSI")
                return View();
            return Redirect("Home");
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
