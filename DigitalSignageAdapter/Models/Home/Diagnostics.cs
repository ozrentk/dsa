using AdapterDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class Diagnostics
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool CacheOnly { get; set; }
        public AggregatedData[] Data { get; set; }
        public string[] BusinessList { get; set; }
        public string[] LineList { get; set; }
    }
}