using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Shared
{
    public class TimeZoneHelper
    {
        public static DateTime ClientToUtc(DateTime clientTime, int offset)
        {
            return clientTime.AddHours(-offset);
        }

        public static DateTime UtcToClient(DateTime utcTime, int offset)
        {
            return utcTime.AddHours(offset);
        }

        public static DateTime ClientCurrentTime(int offset)
        {
            DateTime clientCurrentTime;
            var freezeTime = ConfigurationManager.AppSettings["debug:freezeTime"];
            if (string.IsNullOrEmpty(freezeTime))
                clientCurrentTime = UtcToClient(DateTime.UtcNow, offset);
            else
                clientCurrentTime = DateTime.Parse(freezeTime);
            DateTime clientToday = clientCurrentTime.Date;

            return clientCurrentTime;
        }

    }
}