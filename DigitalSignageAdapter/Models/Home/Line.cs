using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class Line
    {
        public int BusinessId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public List<Shared.Employee> Employees { get; set; }
    }
}