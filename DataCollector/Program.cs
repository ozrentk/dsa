using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollector
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static bool _cfgCollectData = false;
        private static bool _cfgRunCleanup = false;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            log.Debug("log4net configured");

            var cfgCollectData = ConfigurationManager.AppSettings["my:collectData"];
            var cfgRunCleanup = ConfigurationManager.AppSettings["my:runCleanup"];

            if (cfgCollectData != null)
                bool.TryParse(cfgCollectData, out _cfgCollectData);

            if (cfgRunCleanup != null)
                bool.TryParse(cfgRunCleanup, out _cfgRunCleanup);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            log.Info("*** Execute: data collection ***");

            if (!_cfgCollectData)
            {
                log.Info("Data collection not enabled");
            }
            else
            {
                try
                {
                    ExternalData.DataCollector.Collect();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("job failed: {0}", ex);
                }
            }

            log.Info("*** Execute: data cleanup ***");

            if (!_cfgRunCleanup)
            {
                log.Info("Data cleanup not enabled");
            }
            else
            {
                try
                {
                    ExternalData.DataCleaner.DoCleanup();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("job failed: {0}", ex);
                }
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            log.InfoFormat("*** Total running time: {0} ***", watch.Elapsed.ToString());
        }
    }
}
