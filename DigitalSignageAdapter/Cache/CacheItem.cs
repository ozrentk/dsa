using DigitalSignageAdapter.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Cache
{
    public class CacheItem
    {
        public int BusinessId { get; set; }

        public int LineId { get; set; }

        public DateTime Timestamp { get; set; }

        public List<DataItem> DataItemList { get; set; }
    }
}