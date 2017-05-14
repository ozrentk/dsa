using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Home
{
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Code { get; set; }
        public bool IsActive { get; set; }
        public List<Line> Lines { get; set; }
    }
}