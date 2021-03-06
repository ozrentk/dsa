﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalData
{
    public class DataItem
    {
        public int? BusinessId { get; set; }
        public int? LineId { get; set; }
        //public int? BusinessCode { get; set; }
        //public int? LineCode { get; set; }
        public int? CalledById { get; set; }
        public int? QueueId { get; set; }

        public int? ServiceId { get; set; }
        public int? AnalyticId { get; set; }
        public int? ServicedById { get; set; }

        public string CalledByName { get; set; }
        public string ServicedByName { get; set; }


        public string Name { get; set; }
        public int? Verification { get; set; }
        public DateTime? Serviced { get; set; }
        public DateTime? Called { get; set; }
        public DateTime? Entered { get; set; }

        public TimeSpan WaitTime
        {
            get
            {
                return (Called.HasValue && Entered.HasValue) ? Called.Value - Entered.Value : TimeSpan.Zero;
            }
        }

        public TimeSpan ServiceTime
        {
            get
            {
                return (Serviced.HasValue && Called.HasValue) ? Serviced.Value - Called.Value : TimeSpan.Zero;
            }
        }

        public bool IsCached { get; set; }

        public override string ToString()
        {
            return String.Format("|{0,4}|{1,6}|{2,20:yyyy-MM-dd HH:mm:ss}|{3,20:yyyy-MM-dd HH:mm:ss}|{4,-20}|{5,20:yyyy-MM-dd HH:mm:ss}|{6,-20}|{7,10}|",
                LineId,
                BusinessId,
                Entered,
                Called, 
                CalledByName,
                Serviced,
                ServicedByName,
                QueueId);
        }
    }

}