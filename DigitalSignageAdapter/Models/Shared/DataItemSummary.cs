using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Shared
{
    public class DataItemSummary
    {
        public int totalItems { get; set; }
        public TimeSpan avgWaitTime { get; set; }
        public TimeSpan avgServiceTime { get; set; }
    }

}