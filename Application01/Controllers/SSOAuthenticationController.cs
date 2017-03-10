using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Application01.helpers;
using Application01.Classes;

namespace Application01.Controllers
{
    public class SSOAuthenticationController : BaseController
    {
        // GET: SSOAuthentication
        public ActionResult Index(string errorMessage = "")
        {
            if (errorMessage != null)
                ViewBag.ErrorMessage = errorMessage;

            return View();
        }

        public ActionResult SingleLogout()
        {
            if (IsSessionExpired()) { return NavigateToLogin(); }
            return View();
        }

        [HttpGet]
        public ActionResult RedirectToIdpServer(string reason)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (reason == "login")
            {
                AuthRequest req = new AuthRequest();
                result.Add("SAMLRequest", req.GetRequest(AuthRequest.AuthRequestFormat.Base64));
                result.Add("IDPURL", Configuration.idp_sso_target_url);
                return Json(result, "application/json", JsonRequestBehavior.AllowGet);
            }
            else
            {
                LogoutAuthRequest req = new LogoutAuthRequest();
                result.Add("SAMLRequest", req.GetLogoutRequest(LogoutAuthRequest.AuthRequestFormat.Base64));
                result.Add("IDPURL", Configuration.idp_sso_target_url);
                return Json(result, "application/json", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void Logout()
        {
            string errorMessage = string.Empty;
            Classes.Response samlResponse = new Classes.Response();
            samlResponse.LoadXmlFromBase64(Request.Form["SAMLResponse"]);
            if (!samlResponse.IsValid())
            {
                Response.Redirect("~/SSOAuthentication/Index?errorMessage=InvalidSSOResponse");
            }

            string status = samlResponse.GetLogoutStatus();
            if (status == "urn:oasis:names:tc:SAML:2.0:status:Success")
            {
                Session.RemoveAll();
                Response.Redirect("~/SSOAuthentication/Index");
            }
            else
            {
                Response.Redirect("~/SSOAuthentication/Index?errorMessage=SingleLogoutFail");
            }

        }

        [HttpPost]
        public void Login()
        {
            string errorMessage = string.Empty;

            Classes.Response samlResponse = new Classes.Response();
            samlResponse.LoadXmlFromBase64(Request.Form["SAMLResponse"]);
            if (!samlResponse.IsValid())
            {
                Response.Redirect("~/SSOAuthentication/Index?errorMessage=InvalidSSOResponse");
            }

            ApplicationSession.SSONameId = samlResponse.GetNameID();
            ApplicationSession.SSOSessionIndex = samlResponse.GetSessionIndex();

                AuthUserConfig AuthUser = new AuthUserConfig();
                AuthUser.LoginId = samlResponse.GetAttribute("username");
                //AuthUser.FirstName = samlResponse.GetAttribute("firstName");
                //AuthUser.LastName = samlResponse.GetAttribute("lastName");
                //AuthUser.RoleName = samlResponse.GetAttribute("role");
                AuthUser.NameId = samlResponse.GetNameID();
                AuthUser.SessionIndex = samlResponse.GetSessionIndex();
                ApplicationSession.AuthUser = AuthUser;
                Response.Redirect("/Home/Index?name="+ AuthUser.LoginId); 
        }
    }
}