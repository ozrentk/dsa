using DigitalSignageAdapter.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class CompareItem
    {
        public int ItemNumber { get; set; }

        // Combo selection data (all items and seleced items)
        public Business SelectedBusiness { get; set; }
        public List<Line> LineList { get; set; }
        public List<Line> SelectedLineList { get; set; }
        public List<Employee> EmployeeList { get; set; }
        public Employee SelectedEmployee { get; set; }

        // Aggregated data
        public AdapterDb.AggregatedData AggregatedData { get; set; }
    }
}