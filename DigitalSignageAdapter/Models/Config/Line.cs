using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class Line
    {
        public string BusinessName { get; set; }
        public DateTime? Created { get; set; }
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public bool IsDelete { get; set; }
        public bool IsFailed { get; set; }
    }
}