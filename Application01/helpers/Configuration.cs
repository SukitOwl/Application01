using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Application01.Classes;

namespace Application01.helpers
{
    public class Configuration
    {
        public static string GetAppSetting(string key)
        {
            return WebConfigurationManager.AppSettings[key];
        }

        public static string SSONameId
        {
            get { return ApplicationSession.SSONameId; }
        }

        public static string SSOSessionIndex
        {
            get { return ApplicationSession.SSOSessionIndex; }
        }

        public static AuthUserConfig AuthUser
        {
            get { return ApplicationSession.AuthUser; }
        }

        public static string sp_sso_target_url
        {
            get { string val = GetAppSetting("sp_sso_target_url"); return (string.IsNullOrWhiteSpace(val)) ? "" : val.Trim(); }
        }

        public static string sso_issuer
        {
            get { string val = GetAppSetting("sso_issuer"); return (string.IsNullOrWhiteSpace(val)) ? "" : val.Trim(); }
        }

        public static string idp_sso_target_url
        {
            get { string val = GetAppSetting("idp_sso_target_url"); return (string.IsNullOrWhiteSpace(val)) ? "" : val.Trim(); }
        }

        public static string idp_sso_destination_url
        {
            get { string val = GetAppSetting("idp_sso_destination_url"); return (string.IsNullOrWhiteSpace(val)) ? "" : val.Trim(); }
        }

        public static string sso_certificate
        {
            get { string val = GetAppSetting("sso_certificate"); return (string.IsNullOrWhiteSpace(val)) ? "" : val.Trim(); }
        }

        public static string idp_login_url
        {
            get { string val = GetAppSetting("idp_login_url"); return (string.IsNullOrWhiteSpace(val)) ? "" : val.Trim(); }
        }

        public static string sso_cer
        {
            get { string val = GetAppSetting("sso_cer"); return (string.IsNullOrWhiteSpace(val)) ? "" : val.Trim(); }
        }

        public static string sso_cer_pass
        {
            get { string val = GetAppSetting("sso_cer_pass"); return (string.IsNullOrWhiteSpace(val)) ? "" : val.Trim(); }
        }
    }
}