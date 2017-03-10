using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Application01.helpers;

namespace Application01.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index(string name="")
        {
            if (IsSessionExpired()) { return NavigateToLogin(); }

            ViewBag.name = Configuration.AuthUser.LoginId;
            return View();
        }

        public ActionResult About()
        {
            if (IsSessionExpired()) { return NavigateToLogin(); }

            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            if (IsSessionExpired()) { return NavigateToLogin(); }

            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}