using ExternalData;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.ScheduledJobs
{
    public class ExternalDataCollectionJob : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExternalDataCollectionJob));

        private bool _cfgCollectData = false;
        private bool _cfgRunCleanup = false;

        public ExternalDataCollectionJob()
        {
            var cfgCollectData = ConfigurationManager.AppSettings["my:collectData"];
            var cfgRunCleanup = ConfigurationManager.AppSettings["my:runCleanup"];

            if (cfgCollectData != null)
                bool.TryParse(cfgCollectData, out _cfgCollectData);

            if (cfgRunCleanup != null)
                bool.TryParse(cfgRunCleanup, out _cfgRunCleanup);

        }

        /// <summary>
        /// Backup web service data to database
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            log.Info("Execute task: data collection...");

            if (!_cfgCollectData)
            {
                log.Info("Data collection not enabled");
                return;
            }

            try
            {
                DataCollector.Collect();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("job failed: {0}", ex);
            }

            log.Info("Execute task: data cleanup...");

            if (!_cfgRunCleanup)
            {
                log.Info("Data cleanup not enabled");
                return;
            }

            try
            {
                DataCleaner.DoCleanup();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("job failed: {0}", ex);
            }
        }
    }
}