using System;
using System.Collections.Generic;
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
            return UtcToClient(DateTime.UtcNow, offset);
            //return (new DateTime(2017, 3, 7)).AddHours(16);
        }

    }
}