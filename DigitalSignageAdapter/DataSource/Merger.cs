using AdapterDb;
using AutoMapper;
using DigitalSignageAdapter.Cache;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.DataSource
{
    public static class Merger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Merger));
        private static CacheEngine _cache = CacheEngine.Instance;
        private static object _cacheLock = new object();

        private static List<Models.Shared.DataItem> FilterCachedItems(DateTime timeFrom, DateTime timeTo, List<Models.Shared.DataItem> allCachedItems)
        {
            // Offset each item by number of hours in configuration
            //OffsetItems(allCachedItems);

            var cachedItems = allCachedItems.Where(i => timeFrom <= i.Entered && i.Entered <= timeTo).ToList();
            // TODO: diagnostic purposes only - debug this? leave it here?
            var itemsBeforeInterval = allCachedItems.Where(i => i.Entered < timeFrom).ToList();
            var itemsAfterInterval = allCachedItems.Where(i => i.Entered > timeTo).ToList();
            log.DebugFormat("interval: from {0} to {1}", timeFrom, timeTo);
            log.DebugFormat("there are {0} in the inteval ({1} items before and {2} items after)", cachedItems.Count, itemsBeforeInterval.Count, itemsAfterInterval.Count);

            if (itemsBeforeInterval.Count > 0 || itemsAfterInterval.Count > 0)
            {
                if (itemsBeforeInterval.Count > 0)
                {
                    log.DebugFormat("earliest item before: {0}", itemsBeforeInterval.OrderBy(i => i.Entered).FirstOrDefault().Entered);
                }
                if (itemsAfterInterval.Count > 0)
                {
                    log.DebugFormat("latest item after: {0}", itemsAfterInterval.OrderByDescending(i => i.Entered).FirstOrDefault().Entered);
                }
            }

            return cachedItems;
        }

        public static List<Models.Shared.DataItem> GetDataItems(List<Business> businessList, DateTime timeFrom, DateTime timeTo, bool realTimeOnly)
        {
            var businessIds = businessList.Select(b => b.Id).Distinct().ToArray();

            // Look for the "live" items
            var allCachedItems = CacheEngine.Instance.GetItemList(businessIds);
            List<Models.Shared.DataItem> cachedItems = 
                FilterCachedItems(timeFrom, timeTo, allCachedItems)
                    .OrderBy(i => i.QueueId)
                    .ToList();

            if (realTimeOnly)
                return cachedItems;

            // Look for the rest of items in the DB
            var oldestQueueIds = (from bid in businessIds
                                  select new
                                  {
                                     id = bid,
                                     queueId = cachedItems.Where(i => i.BusinessId == bid).Select(i => i.QueueId).OrderBy(i => i).FirstOrDefault()
                                  }).ToDictionary(k => k.id, v => v.queueId);

            var dbStoredItems = AdapterDb.Database.GetDataItems(businessIds, oldestQueueIds, timeFrom, timeTo);
            List<Models.Shared.DataItem> storedItems = Mapper.Map<List<AdapterDb.DataItem>, List<Models.Shared.DataItem>>(dbStoredItems);
            log.DebugFormat("number of internal items: {0}", storedItems.Count);

            // Concatenate everything
            var allItems = cachedItems.Union(storedItems).ToList();

            // Create a summary item
            //Models.Shared.DataItemSummary summary = CreateDataItemSummary(items);
            //items.Add(new Models.Shared.DataItem()
            //{
            //    name = String.Format("Total: {0} customers", summary.totalItems),
            //    waitTime = summary.avgWaitTime,
            //    serviceTime = summary.avgServiceTime,
            //});

            return allItems;
        }

        public static List<Models.Shared.DataItem> GetDataItems(int businessId, int lineId, DateTime timeFrom, DateTime timeTo)
        {
            // Look for the "live" items
            var allCachedItems = CacheEngine.Instance.GetItemList(businessId, lineId);
            List<Models.Shared.DataItem> items = 
                FilterCachedItems(timeFrom, timeTo, allCachedItems)
                    .OrderBy(i => i.QueueId)
                    .ToList();

            // Look for the rest of items in the DB (younger than queueId)
            var oldestQueueId = items.Select(i => i.QueueId).FirstOrDefault();
            var dbStoredItems = AdapterDb.Database.GetDataItems(businessId, lineId, timeFrom, timeTo, oldestQueueId);
            List<Models.Shared.DataItem> storedItems = Mapper.Map<List<AdapterDb.DataItem>, List<Models.Shared.DataItem>>(dbStoredItems);
            log.DebugFormat("number of internal items: {0}", storedItems.Count);

            // Concatenate everything
            List<Models.Shared.DataItem> allItems = storedItems.Concat(items).ToList();

            // Create a summary item
            Models.Shared.DataItemSummary summary = CreateDataItemSummary(allItems);
            allItems.Add(new Models.Shared.DataItem()
            {
                Name = String.Format("Total: {0} customers", summary.totalItems),
            });

            return allItems;
        }

        //private static void OffsetItems(List<Models.Shared.DataItem> cleanItems)
        //{
        //    var dbCfgItems = Database.GetConfigItems();
        //    var timeOffsetHours = int.Parse(dbCfgItems.First(i => i.Name.Equals("TimeOffsetHours")).Value);
        //    cleanItems.ForEach(
        //        i =>
        //        {
        //            if (i.entered.HasValue)
        //                i.entered = i.entered.Value.AddHours(timeOffsetHours);

        //            if (i.called.HasValue)
        //                i.called = i.called.Value.AddHours(timeOffsetHours);

        //            if (i.serviced.HasValue)
        //                i.serviced = i.serviced.Value.AddHours(timeOffsetHours);
        //        });
        //}

        private static List<Models.Shared.DataItem> DistinctItems(List<Models.Shared.DataItem> internalItems, List<Models.Shared.DataItem> externalItems)
        {
            // All items
            var items = internalItems.Concat(externalItems).ToList();

            // Distinct ('clean') items
            var cleanItems = (from i in items
                              group i by new { i.LineId, i.BusinessId, i.QueueId } into g
                              let item1 = g.First()
                              orderby item1.QueueId
                              select item1).ToList();
            log.DebugFormat("number of clean items: {0}", cleanItems.Count);
            return cleanItems;
        }

        private static Models.Shared.DataItemSummary CreateDataItemSummary(List<Models.Shared.DataItem> dataItems)
        {
            var total_items = dataItems.Count;
            var avg_wait_time_secs = dataItems.Select(i => i.WaitTime.TotalSeconds).Sum() * 1.0 / total_items;
            var avg_wait_time = new TimeSpan(0, 0, (int)avg_wait_time_secs);
            var avg_service_time_secs = dataItems.Select(i => i.ServiceTime.TotalSeconds).Sum() * 1.0 / total_items;
            var avg_service_time = new TimeSpan(0, 0, (int)avg_service_time_secs);

            return new Models.Shared.DataItemSummary()
            {
                avgWaitTime = avg_wait_time,
                avgServiceTime = avg_service_time,
                totalItems = total_items
            };
        }

    }
}