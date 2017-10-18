using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterDb
{
    public class EmployeeTimes
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int DailyServiceTimeSec { get; set; }
        public int MonthlyServiceTimeSec { get; set; }
        public int YearlyServiceTimeSec { get; set; }
    }
}
