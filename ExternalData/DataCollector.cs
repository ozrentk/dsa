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
    public class DataCollector
    {
        class MergeItemsRetVal {
            int? InsertedId { get; set; }
            int? DeletedId { get; set; }
            string ActionName { get; set; }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(DataCollector));

        private static object listLocker = new object();

        private static void Write(List<DataItem> items)
        {
            var tblSyncItems = new System.Data.DataTable();
            tblSyncItems.Columns.Add("BusinessId", typeof(int));
            tblSyncItems.Columns.Add("LineId", typeof(int));
            tblSyncItems.Columns.Add("ServiceId", typeof(int));
            tblSyncItems.Columns.Add("AnalyticId", typeof(int));
            tblSyncItems.Columns.Add("QueueId", typeof(int));
            tblSyncItems.Columns.Add("Name", typeof(string));
            tblSyncItems.Columns.Add("Verification", typeof(string));
            //tblSyncItems.Columns.Add("Serviced", typeof(DateTime?));
            tblSyncItems.Columns.Add("Serviced", Nullable.GetUnderlyingType(typeof(DateTime?)));
            tblSyncItems.Columns.Add("ServicedByName", typeof(string));
            //tblSyncItems.Columns.Add("Called", typeof(DateTime));
            tblSyncItems.Columns.Add("Called", Nullable.GetUnderlyingType(typeof(DateTime?)));
            tblSyncItems.Columns.Add("CalledByName", typeof(string));
            //tblSyncItems.Columns.Add("Entered", typeof(DateTime?));
            tblSyncItems.Columns.Add("Entered", Nullable.GetUnderlyingType(typeof(DateTime?)));

            items.ForEach(i =>
            {
                tblSyncItems.Rows.Add(new object[12] {
                    i.BusinessId,
                    i.LineId,
                    i.ServiceId,
                    i.AnalyticId,
                    i.QueueId,
                    i.Name,
                    i.Verification,
                    i.Serviced,
                    i.ServicedByName,
                    i.Called,
                    i.CalledByName,
                    i.Entered
                });

                if (i.QueueId == 188985)
                    log.DebugFormat("QUEUEID 188985: ['{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}']",
                        i.BusinessId,
                        i.LineId,
                        i.ServiceId,
                        i.AnalyticId,
                        i.QueueId,
                        i.Name,
                        i.Verification,
                        i.Serviced,
                        i.ServicedByName,
                        i.Called,
                        i.CalledByName,
                        i.Entered);
            });


            using (var db = new AdapterDbEntities())
            {
                SqlParameter syncItemsParam = new SqlParameter("@items", tblSyncItems);
                syncItemsParam.TypeName = "DataItem";
                syncItemsParam.SqlDbType = SqlDbType.Structured;

                log.DebugFormat("Merging {0} items with database...", tblSyncItems.Rows.Count);
                var retVal = db.Database.SqlQuery<MergeItemsRetVal>("EXEC MergeItems @items", syncItemsParam).ToList();
                log.DebugFormat("{0} items merged into {1} items!", tblSyncItems.Rows.Count, retVal.Count);
            }
        }


        public static void Collect()
        {
            log.Info("START: DataCollector.Collect()");

            try
            {
                var businessList = Database.GetBusinessLineList(null);

                List<Task> tasks = new List<Task>();
                List<DataItem> allItems = new List<DataItem>();

                foreach (var biz in businessList)
                {
                    log.DebugFormat("Creating data collection tasks for business (id={0}, code={1}, name={2})", biz.Id, biz.Code, biz.Name);

                    foreach (var ln in biz.Line)
                    {
                        log.DebugFormat("Creating data collection task for line (id={0}, code={1}, name={2})", ln.Id, ln.Code, ln.Name);

                        Task t = Task.Run(() =>
                        {
                            log.DebugFormat("Starting data collection task for business/line {0}/{1}...", biz.Code, ln.Code);

                            var items = Fetcher.FetchItems(biz.Code, ln.Code, 2);
                            log.DebugFormat("Collected {0} items", items.Count);
                            lock (listLocker)
                            {
                                allItems.AddRange(items);
                            }
                            log.DebugFormat("Finished data collection task for business/line {0}/{1}!", biz.Id, ln.Id);
                        });

                        tasks.Add(t);
                    }
                }

                Task ending = Task.WhenAll(tasks);
                try
                {
                    ending.Wait();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Data collection task failed: {0}", ex);
                    throw ex;
                }

                log.DebugFormat("Collected total of {0} items", allItems.Count);
                Write(allItems);

                //if (ending.Status == TaskStatus.RanToCompletion)
                //    CacheEngine.Instance.State = CacheEngineState.Ready;
                //else if (ending.Status == TaskStatus.Faulted)
                //    CacheEngine.Instance.State = CacheEngineState.Invalid;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DbWriter.Write() failed: {0}", ex);
            }

            log.Info("END: DataCollector.Collect()");
        }

    }
}
