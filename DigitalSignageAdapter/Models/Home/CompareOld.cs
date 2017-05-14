using DigitalSignageAdapter.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class CompareOld
    {
        // Data entered/selected for comparison
        public TimeEntryType TimeEntryType { get; set; }
        //public DateTime UtcTimeFrom { get; set; }
        //public DateTime UtcTimeTo { get; set; }

        public DateTime ClientTimeFrom { get; set; }
        public DateTime ClientTimeTo { get; set; }
        public int TimeInDays { get; set; }
        public List<Business> BusinessList { get; set; }
        public List<Line> LineList { get; set; }

        // Data aggregated for comparison
        public List<AdapterDb.AggregatedData> AggregatedByBusiness { get; set; }
        public List<AdapterDb.AggregatedData> AggregatedByLineName { get; set; }
        public AdapterDb.AggregatedData AggregatedTotal { get; set; }

        // Combo selection data
        public List<Business> AllBusinessList { get; set; }
        public List<Line> AllLines { get; set; }
        public List<Employee> AllEmployees { get; set; }

        // Add/remove ids and indexes of business
        public int? AddBusinessId { get; set; }
        public int? RemoveBusinessIdx { get; set; }

        // Add/remove ids and indexes of line
        public int? AddLineBusinessIdx { get; set; }
        public int? AddLineId { get; set; }
        public int? RemoveLineIdx { get; set; }

        // Add/remove ids and indexes of employee
        public int? AddEmployeeLineIdx { get; set; }
        public int? AddEmployeeId { get; set; }
        public int? RemoveEmployeeIdx { get; set; }

        // Data getters
        //public AdapterDb.AggregatedData GetBusinessData(int businessId)
        //{
        //    return AggregatedByBusiness.Where(b => b.BusinessId == businessId).FirstOrDefault();
        //}

    }
}