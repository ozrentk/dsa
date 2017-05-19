using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSA.WEB.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string ViewName(this HtmlHelper html)
        {
            var virtPath = (html.ViewDataContainer as System.Web.WebPages.WebPageBase).VirtualPath;
            return Path.GetFileNameWithoutExtension(virtPath);
        }
    }
}