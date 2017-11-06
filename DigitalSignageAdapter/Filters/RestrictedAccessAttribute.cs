using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalSignageAdapter.Extensions;
using System.Reflection;

namespace DigitalSignageAdapter.Filters
{
    public class RestrictedAccessAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new HttpStatusCodeResult(403);
            else
                filterContext.Result = new HttpUnauthorizedResult();
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            var rd = httpContext.Request.RequestContext.RouteData;
            string currentController = rd.GetRequiredString("controller");
            string currentAction = rd.GetRequiredString("action");
            var method = httpContext.Request.HttpMethod;

            var controllerFullName = 
                $"{nameof(DigitalSignageAdapter)}.Controllers.{currentController}Controller";
            var controllerType = Assembly.GetExecutingAssembly().GetType(controllerFullName);
            var actionExists = 
                controllerType != null && 
                controllerType.GetMethods().Where(m => m.Name.Equals(currentAction, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;

            // If action exists, we just let this request pass through, to reach the 404 page
            if (!actionExists)
                return true;

            return httpContext.User.IsAllowed(currentController, currentAction, method);
        }
    }
}