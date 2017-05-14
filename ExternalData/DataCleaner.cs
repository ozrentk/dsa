using AdapterDb;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalData
{
    public class DataCleaner
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DataCleaner));

        public static void DoCleanup()
        {
            log.Info("START: DataCleaner.DoCleanup()");

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

            log.Info("END: DataCleaner.DoCleanup()");
        }
    }
}
