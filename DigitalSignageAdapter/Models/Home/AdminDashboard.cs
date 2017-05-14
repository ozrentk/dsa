using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class AdminDashboard
    {
        public MultipleBusinesses BusinessData { get; set; }
        public List<EmployeeTimes> EmployeeData { get; set; }
        public List<User> UserList { get; set; }
        public int? DeactivateUserId { get; set; }
    }
}