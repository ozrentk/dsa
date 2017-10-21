using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalSignageAdapter.Filters
{
    public class TimeZoneActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Object tzOffsetObj = filterContext.HttpContext.Session["timeZoneOffset"];
            if (tzOffsetObj == null)
            {
                var dict = new System.Web.Routing.RouteValueDictionary(
                    new { 
                        controller = "TimeZone", 
                        action = "RefreshOffset",
                        returnUrl = filterContext.HttpContext.Request.Url
                    });
                filterContext.Result = new RedirectToRouteResult(dict);
                return;
            }

            string tzOffsetStr = tzOffsetObj.ToString();
            int tzOffset;
            if (int.TryParse(tzOffsetStr, out tzOffset))
            {
                filterContext.Controller.ViewBag.TimeZoneOffset = tzOffset;
            }
        }
    }
}