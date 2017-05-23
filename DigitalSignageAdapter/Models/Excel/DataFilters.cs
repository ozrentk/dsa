using DigitalSignageAdapter.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Excel
{
    public class DataFilters
    {
        public int BusinessId { get; set; }
        public int? LineId { get; set; }
        public int? EmployeeId { get; set; }
        public TimeEntryType TimeEntryType { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Days { get; set; }
        public List<Models.Config.Business> BusinessList { get; set; }
    }
}