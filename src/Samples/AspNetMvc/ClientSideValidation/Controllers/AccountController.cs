using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using ClientSideValidation.Models;
using System.Web.Mvc.Html;
using System.Globalization;


namespace ClientSideValidation.Controllers
{
    [HandleError]
    public class AccountController : Controller
    {

        protected override void Initialize(RequestContext requestContext)
        {

            base.Initialize(requestContext);
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        [HttpGet]
        public ActionResult LogOn()
        {

            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            return View(model);
        }
    }
}
