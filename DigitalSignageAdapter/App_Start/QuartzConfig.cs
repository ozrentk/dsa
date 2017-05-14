using AdapterDb;
using DigitalSignageAdapter.ScheduledJobs;
using log4net;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalSignageAdapter
{
    public class QuartzConfig
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(QuartzConfig));

        public static void Configure()
        {
            //var cfgCronExp = ConfigurationManager.AppSettings["my:backupCronExpression"];

            var dbCfgItems = Database.GetConfigItems();
            var dataCollectionSchedule = dbCfgItems.First(i => i.Name.Equals("DataCollectionCronSchedule")).Value;

            // construct a scheduler factory
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory schedFact = new StdSchedulerFactory();

            // get a scheduler
            IScheduler sched = schedFact.GetScheduler();
            sched.Start();

            ScheduleDataCollectionJob(dataCollectionSchedule, sched);
            //ScheduleCacheJob(60, sched);
        }

        private static void ScheduleDataCollectionJob(string dataCollectionSchedule, IScheduler sched)
        {
            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<ExternalDataCollectionJob>()
                .WithIdentity("dataCollectionJob", "group1")
                .Build();

            log.DebugFormat("configuring data collection job using CRON expression {0}...", dataCollectionSchedule);

            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("dataCollectionTrigger", "group1")
              .WithCronSchedule(dataCollectionSchedule)
              .Build();

            sched.ScheduleJob(job, trigger);
        }

        //private static void ScheduleCacheJob(int seconds, IScheduler sched)
        //{
        //    // define the job and tie it to our HelloJob class
        //    IJobDetail job = JobBuilder.Create<CacheJob>()
        //        .WithIdentity("cacheJob", "group2")
        //        .Build();

        //    log.DebugFormat("configuring cache job using simple schedule {0}...", seconds);

        //    ITrigger trigger = TriggerBuilder.Create()
        //        .WithIdentity("cacheTrigger", "group2")
        //        .WithSimpleSchedule(s => s
        //            .WithIntervalInSeconds(seconds)
        //            .RepeatForever())
        //      .Build();

        //    sched.ScheduleJob(job, trigger);
        //}
    }
}
