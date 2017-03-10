using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Application01.helpers;

namespace Application01.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsSessionExpired()
        {
            //AuthUserConfig authUser = Configuration.AuthUser;
            return Configuration.AuthUser == null;
        }

        protected ActionResult NavigateToLogin()
        {
            //string function = string.Format("window.location='/SSOAuthentication/Index");
            return RedirectToAction("Index", "SSOAuthentication");
        }
    }
}