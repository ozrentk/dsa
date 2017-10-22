using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace DigitalSignageAdapter.Extensions
{
    public static class IPrincipalExtensions
    {
        public static bool IsAllowed(this IPrincipal user, string controller, string action, string method)
        {
            return ActivityRestrictions.Instance.IsAllowed(user.Identity.Name, controller, action, method);
        }

        public static bool IsAllowedGet(this IPrincipal user, string controller, string action)
        {
            return ActivityRestrictions.Instance.IsAllowed(user.Identity.Name, controller, action, "GET");
        }

        public static bool IsAllowedPost(this IPrincipal user, string controller, string action)
        {
            return ActivityRestrictions.Instance.IsAllowed(user.Identity.Name, controller, action, "POST");
        }
    }
}