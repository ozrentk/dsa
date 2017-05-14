using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class SingleBusiness
    {
        //public DateTime UtcTimeFrom { get; set; }
        //public DateTime UtcTimeTo { get; set; }
        public DateTime ClientTimeFrom { get; set; }
        public DateTime ClientTimeTo { get; set; }
        public string BusinessName { get; set; }
        public bool UserHasMultipleBusinesses { get; set; }
        public List<AdapterDb.AggregatedData> DataByLine { get; set; }
        public AdapterDb.AggregatedData TotalData { get; set; }
        public List<EmployeeTimes> EmployeeData { get; set; }
    }
}