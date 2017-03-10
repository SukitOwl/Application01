using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Application01.Classes
{
    public class AuthUserConfig
    {
        public string LoginId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }
        public string NameId { get; set; }
        public string SessionIndex { get; set; }

        public AuthUserConfig()
        {
            LoginId = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            RoleName = string.Empty;
            NameId = string.Empty;
            SessionIndex = string.Empty;
        }
    }
}