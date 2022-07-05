using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using osb.Helpers;
using osb.Models;
using osb.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace osb.Controllers
{
    public class ErrorController : BaseController
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodehandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var errorViewModel = new ErrorViewModel();
            errorViewModel.baseURL = "https://" + this.Request.Host;
            errorViewModel.randomBeatmap = DummyHelper.GetRandomBeatmap();
            if (statusCodeResult != null)
            {
                errorViewModel.RequestId = statusCodeResult.OriginalPath;
            }
            switch (statusCode)
            {
                case 404:
                    
                    return View("NotFound", errorViewModel);
            }
            return View("NotFound", errorViewModel);
        }

        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel();
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            errorViewModel.ErrorMessage = exceptionDetails.Error.Message;
            return View("InternalError");
        }
    }
}
