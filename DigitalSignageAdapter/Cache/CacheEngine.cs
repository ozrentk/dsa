using AdapterDb;
using AutoMapper;
using DigitalSignageAdapter.DataSource;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CacheDictionary =
    System.Collections.Generic.Dictionary<int,
        System.Collections.Generic.Dictionary<int,
            DigitalSignageAdapter.Cache.CacheItem>>;

namespace DigitalSignageAdapter.Cache
{
    public enum CacheEngineState : byte
    {
        NotInitialized = 1,
        WarmingUp = 2,
        Ready = 3,
        Refreshing = 4,
        Invalid = 5
    }

    public class CacheEngine
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CacheEngine));
        private static readonly CacheEngine instance = new CacheEngine() { State = CacheEngineState.NotInitialized };

        private CacheDictionary _data;
        private CacheEngineState _state;
        private int? lastQueueId;
        private object _rwLock = new object();

        private CacheEngine()
        {
            _data = new CacheDictionary();
        }

        public static CacheEngine Instance
        {
            get
            {
                return instance;
            }
        }

        public CacheEngineState State
        {
            get
            {
                return _state;
            }

            set
            {
                lock (_rwLock)
                {
                    _state = value;
                }
            }
        }

        public void Clear()
        {
            lock (_rwLock)
            {
                _data.Clear();
            }
        }

        public bool TryRefresh()
        {
            var lastStoredQueueId = DbProxy.GetLastQueueId();

            if (!lastStoredQueueId.HasValue)
            {
                log.DebugFormat("No data items in database");
                _state = CacheEngineState.Ready;
                return true;
            }
            else if (_state == CacheEngineState.NotInitialized)
            {
                log.DebugFormat("Warming up cache...");

                _state = CacheEngineState.WarmingUp;

                DoRefreshInternal();

                return true;
            }
            else if (_state == CacheEngineState.Ready && lastQueueId.HasValue)
            {
                log.DebugFormat("Refreshing cache...");

                if (lastQueueId == lastStoredQueueId)
                {
                    log.DebugFormat("Nothing to refresh, last queue id: {0}", lastQueueId);
                    return true;
                }

                _state = CacheEngineState.Refreshing;

                DoRefreshInternal();

                return true;
            }
            else
            {
                log.DebugFormat("Can't refresh in '{0}' state", _state.ToString());

                return false;
            }
        }

        private void DoRefreshInternal()
        {
            var businessList = Database.GetBusinessLineList(null);

            List<Task> tasks = new List<Task>();
            foreach (var biz in businessList)
            {
                foreach (var ln in biz.Line)
                {
                    log.DebugFormat("Creating cache task for business {0}, line {1}", biz.Id, ln.Id);

                    tasks.Add(
                        new Task(() =>
                        {
                            var items = ExternalData.Fetch(biz.Id, ln.Id, 2);
                            if (items.Count == 0)
                            {
                                log.DebugFormat("Nothing found in external data for line {0}", ln.Id);
                            }
                            else
                            {
                                var lastLineQueueId = items.OrderByDescending(i => i.QueueId).First().QueueId;
                                var lastStoredLineQueueId = Database.GetLastLineQueueId(ln.Id);

                                if (!lastStoredLineQueueId.HasValue)
                                {
                                    log.DebugFormat("Nothing in database for line {0}, going to merge {1} items", ln.Id, items.Count);
                                    var dbItems = Mapper.Map<IEnumerable<Models.Shared.DataItem>, IEnumerable<AdapterDb.DataItem>>(items).ToList();
                                    Database.MergeDataItems(dbItems);
                                    log.DebugFormat("Line {0} merged", ln.Id);
                                }
                                else if (lastLineQueueId.Value > lastStoredLineQueueId.Value)
                                {
                                    log.DebugFormat("External data has new items for line {0}, going to merge {1} items", ln.Id, items.Count);
                                    var dbItems = Mapper.Map<IEnumerable<Models.Shared.DataItem>, IEnumerable<AdapterDb.DataItem>>(items).ToList();
                                    Database.MergeDataItems(dbItems);
                                    log.DebugFormat("Line {0} merged", ln.Id);
                                }
                                else if (lastLineQueueId.Value == lastStoredLineQueueId.Value)
                                {
                                    log.DebugFormat("Nothing to refresh for line {0}, last queue id: {1}, skip refresh from db", ln.Id, lastQueueId);
                                    return;
                                }
                                else if (lastLineQueueId.Value < lastStoredLineQueueId.Value)
                                {
                                    log.ErrorFormat("Line {0}, last external data queue id ({1}) is older than last database queue id ({2})", ln.Id, lastLineQueueId.Value, lastStoredLineQueueId.Value);
                                }
                            }


                            SetItemList(biz.Id, ln.Id, items);

                            log.DebugFormat("Finished cache task for business {0}, line {1}", biz.Id, ln.Id);
                        }));
                }
            }
        }

        public List<Models.Shared.DataItem> GetItemList(int businessId, int lineId)
        {
            CacheItem cacheItem = null;

            lock (_rwLock)
            {
                if (!_data.ContainsKey(businessId))
                    return null;

                if (!_data[businessId].ContainsKey(lineId))
                    return null;

                cacheItem = _data[businessId][lineId];

                // TODO: get new if too old
            }

            return cacheItem.DataItemList;
        }

        public List<Models.Shared.DataItem> GetItemList(int businessId)
        {
            List<Models.Shared.DataItem> items = null;

            lock (_rwLock)
            {
                if (!_data.ContainsKey(businessId))
                    return null;

                items = _data[businessId].SelectMany(d => d.Value.DataItemList).ToList();
            }

            return items;
        }

        public List<Models.Shared.DataItem> GetItemList(int[] businessIds)
        {
            List<Models.Shared.DataItem> items = null;

            lock (_rwLock)
            {
                items = _data.Where(b => businessIds.Contains(b.Key))
                             .SelectMany(b => b.Value)
                             .SelectMany(l => l.Value.DataItemList)
                             .ToList();
            }

            return items;
        }

        public List<Models.Shared.DataItem> GetItemList()
        {
            List<Models.Shared.DataItem> items = null;

            lock (_rwLock)
            {
                items = _data.SelectMany(b => b.Value).SelectMany(l => l.Value.DataItemList).ToList();
            }

            return items;
        }

        public int GetLineCount()
        {
            // Avoid locking - it's just a control count
            return _data.SelectMany(b => b.Value).Select(l => l.Value).Count();
        }

        public void SetItemList(int businessId, int lineId, List<Models.Shared.DataItem> items)
        {
            lock (_rwLock)
            {
                if (!_data.ContainsKey(businessId))
                    _data[businessId] = new Dictionary<int, CacheItem>();

                items.ForEach(i => i.IsCached = true);

                _data[businessId][lineId] = new CacheItem
                {
                    BusinessId = businessId,
                    LineId = lineId,
                    Timestamp = DateTime.UtcNow,
                    DataItemList = items
                };

                log.DebugFormat("Number of items for business {0}, line {1}: {2}", businessId, lineId, items.Count);
            }
        }
    }
}