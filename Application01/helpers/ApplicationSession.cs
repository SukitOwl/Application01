using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Application01.Classes;

namespace Application01.helpers
{
    public class ApplicationSession
    {
        private const string authUser = "AuthUser";
        private const string nameId = "SSONameId";
        private const string sessionIndex = "SSOSessionIndex";

        public static AuthUserConfig AuthUser
        {
            get { return (AuthUserConfig)HttpContext.Current.Session[authUser]; }
            set { HttpContext.Current.Session[authUser] = value; }
        }

        public static string SSONameId
        {
            get { return (string)HttpContext.Current.Session[nameId]; }
            set { HttpContext.Current.Session[nameId] = value; }
        }

        public static string SSOSessionIndex
        {
            get { return (string)HttpContext.Current.Session[sessionIndex]; }
            set { HttpContext.Current.Session[sessionIndex] = value; }
        }
    }
}