using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class EmployeeTimes
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public TimeSpan AverageServiceTime { get; set; }
        public TimeSpan MonthlyServiceTime { get; set; }
        public TimeSpan YearlyServiceTime { get; set; }
    }
}