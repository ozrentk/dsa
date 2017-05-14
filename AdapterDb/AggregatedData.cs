using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdapterDb
{
    public class AggregatedData
    {
        public DateTime? Date { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public int? BusinessId { get; set; }
        public string BusinessName { get; set; }
        public int? LineId { get; set; }
        public string LineName { get; set; }
        public int AverageWaitTime { get; set; }
        public int AverageServiceTime { get; set; }
        public int CustomersWaitingCount { get; set; }
        public int CustomersBeingServicedCount { get; set; }
        public int CustomersServicedCount { get; set; }
        public int CustomersCount { get; set; }
        public bool? IsCached { get; set; }

        public string AverageWaitTimeDisplay
        {
            get
            {
                return TimeSpan.FromSeconds(AverageWaitTime).ToString("c");
            }
        }

        public string AverageServiceTimeDisplay
        {
            get
            {
                return TimeSpan.FromSeconds(AverageServiceTime).ToString("c");
            }
        }
    }
}