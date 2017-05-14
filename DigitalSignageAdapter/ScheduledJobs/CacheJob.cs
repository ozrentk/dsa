using AdapterDb;
using AutoMapper;
using DigitalSignageAdapter.Cache;
using DigitalSignageAdapter.DataSource;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DigitalSignageAdapter.ScheduledJobs
{
    public class CacheJob : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CacheJob));

        public void Execute(IJobExecutionContext context)
        {
            log.Debug("executing job: cache refresh...");

            try
            {
                var businessList = Database.GetBusinessLineList(null);

                List<Task> tasks = new List<Task>();

                foreach (var biz in businessList)
                {
                    foreach (var ln in biz.Line)
                    {
                        log.DebugFormat("starting cache task for business {0}, line {1}...", biz.Id, ln.Id);

                        Task t = new Task(() =>
                        {
                            var items = ExternalData.Fetch(biz.Id, ln.Id, 2);
                            CacheEngine.Instance.SetItemList(biz.Id, ln.Id, items);
                            log.DebugFormat("finished cache task for business {0}, line {1}", biz.Id, ln.Id);
                        });

                        tasks.Add(t);
                    }
                }

                Task ending = Task.WhenAll(tasks);
                try
                {
                    ending.Wait();
                    CacheEngine.Instance.State = CacheEngineState.Ready;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("cache tasks failed: {0}", ex);
                }

                if (ending.Status == TaskStatus.RanToCompletion)
                    CacheEngine.Instance.State = CacheEngineState.Ready;
                else if (ending.Status == TaskStatus.Faulted)
                    CacheEngine.Instance.State = CacheEngineState.Invalid;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("job failed: {0}", ex);
            }

            log.Debug("executed job: cache refresh");
        }
    }
}