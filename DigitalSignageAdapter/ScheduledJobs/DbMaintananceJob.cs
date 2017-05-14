using AdapterDb;
using DigitalSignageAdapter.Models;
using AutoMapper;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DigitalSignageAdapter.DataSource;

namespace DigitalSignageAdapter.ScheduledJobs
{
    public class DbMaintananceJob : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DbMaintananceJob));

        /// <summary>
        /// Backup web service data to database
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            DoBackup();
            DoCleanup();
        }

        private static void DoBackup()
        {
            log.Debug("executing task: database backup from web service...");

            try
            {
                var dbCfgItems = Database.GetConfigItems();
                var backupDays = int.Parse(dbCfgItems.First(i => i.Name.Equals("BackupDays")).Value);

                Database.DoBackup((lineId, businessId) =>
                {
                    var dataItems = ExternalData.Fetch(businessId, lineId, backupDays);
                    var dbDataItems = Mapper.Map<List<AdapterDb.DataItem>>(dataItems);
                    return dbDataItems;
                });
            }
            catch (Exception ex)
            {
                log.ErrorFormat("job failed: {0}", ex);
            }
        }

        private static void DoCleanup()
        {
            log.Debug("executing task: database cleanup...");

            try
            {
                var currentTime = DateTime.UtcNow;

                var dbCfgItems = Database.GetConfigItems();
                var cleanupTreshold = int.Parse(dbCfgItems.First(i => i.Name.Equals("CleanupTreshold")).Value);
                var lastValidTime = currentTime.AddHours(-cleanupTreshold);

                log.DebugFormat("removing items older than {0}", lastValidTime.ToString());

                Database.DoCleanup(lastValidTime: lastValidTime);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("job failed: {0}", ex);
            }
        }
    }
}