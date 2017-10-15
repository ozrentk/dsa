using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DigitalSignageAdapter
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger("TraceLogger");

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            log.Debug("log4net configured");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //QuartzConfig.Configure();
            AutomapperConfig.Configure();

            ModelBinders.Binders.Add(typeof(DateTime), new DigitalSignageAdapter.CustomModelBinders.DateTimeCultureModelBinder());

            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

            log.Debug("application started ok");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Response.Clear();

            if (exception is System.Data.Entity.Core.EntityException)
            {
                // Error number not documented ?!?!?!
                // 0x80131501 ?
                if (exception.Message.Equals("The underlying provider failed on Open."))
                {
                    // Try failover
                    using (NamedPipeClientStream namedPipeClient =
                        new NamedPipeClientStream(
                            ".",
                            "DSA_ls_test_lspipe",
                            PipeDirection.InOut))
                    {
                        try
                        {
                            namedPipeClient.Connect(200);
                        }
                        catch (System.TimeoutException ex)
                        {
                            log.Error("Nothing on the other end of the pipe!");
                        }
                        namedPipeClient.ReadMode = PipeTransmissionMode.Message;

                        var request = new { Command = "Switch", RoleIndicator = "Primary" };
                        string requestJson = JsonConvert.SerializeObject(request);
                        byte[] messageBytes = Encoding.UTF8.GetBytes(requestJson);
                        namedPipeClient.Write(messageBytes, 0, messageBytes.Length);
                    }
                }
            }


            //HttpException httpException = exception as HttpException;
            //if (httpException != null)
            //{
            //    RouteData routeData = new RouteData();
            //    routeData.Values.Add("controller", "Error");
            //    switch (httpException.GetHttpCode())
            //    {
            //        case 404:
            //            // page not found
            //            routeData.Values.Add("action", "HttpError404");
            //            break;
            //        case 500:
            //            // server error
            //            routeData.Values.Add("action", "HttpError500");
            //            break;
            //        default:
            //            routeData.Values.Add("action", "General");
            //            break;
            //    }
            //    routeData.Values.Add("error", exception);
            //    // clear error on server
            //    Server.ClearError();

            //    // at this point how to properly pass route data to error controller?
            //}
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
