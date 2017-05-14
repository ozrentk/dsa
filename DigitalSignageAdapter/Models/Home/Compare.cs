using DigitalSignageAdapter.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class Compare
    {
        // Interval
        public TimeEntryType TimeEntryType { get; set; }
        public int TimeInDays { get; set; }
        public DateTime ClientTimeFrom { get; set; }
        public DateTime ClientTimeTo { get; set; }

        // Combo selection data (all items)
        public List<Business> BusinessList { get; set; }

        // Items
        public List<CompareItem> CompareItems { get; set; }

        // Actions
        public bool ActionIsAdd { get; set; }
        public bool ActionIsRemove { get; set; }
        public bool ActionIsToggle { get; set; }
        public int? ActionItemNumber { get; set; }
        public int? ActionBusinessId { get; set; }
        public int? ActionLineId { get; set; }
        public int? ActionEmployeeId { get; set; }
    }
}