using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DigitalSignageAdapter
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            log.Debug("log4net configured");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            QuartzConfig.Configure();
            AutomapperConfig.Configure();

            ModelBinders.Binders.Add(typeof(DateTime), new DigitalSignageAdapter.CustomModelBinders.DateTimeCultureModelBinder());

            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

            log.Debug("application started ok");
        }

        //protected void Application_BeginRequest(Object sender, EventArgs e)
        //{

        //    var newCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        //    newCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
        //    newCulture.DateTimeFormat.LongDatePattern = "dd/MM/yyyy hh:mm:ss";
        //    newCulture.DateTimeFormat.DateSeparator = "/";
        //    System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
        //}
    }
}
