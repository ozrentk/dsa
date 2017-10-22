using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DigitalSignageAdapter.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString RestrictedActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string wrapper)
        {
            if (HttpContext.Current.User.IsAllowedGet(controllerName, actionName))
            {
                if (string.IsNullOrEmpty(wrapper))
                {
                    return htmlHelper.ActionLink(linkText, actionName, controllerName);
                }
                else
                {
                    TagBuilder wrapEl = new TagBuilder(wrapper);
                    wrapEl.InnerHtml = htmlHelper.ActionLink(linkText, actionName, controllerName).ToHtmlString();
                    var wrappedContent = MvcHtmlString.Create(wrapEl.ToString());
                    return wrappedContent;
                }
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }
        }
    }
}