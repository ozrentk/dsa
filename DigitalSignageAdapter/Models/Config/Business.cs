using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class Business
    {
        public DateTime? Created { get; set; }
        public int Id { get; set; }
        public List<int> UserIds { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public int? TicketId { get; set; }
        public List<Models.Config.Line> Lines { get; set; }
        public bool IsFailed { get; set; }
        public bool IsSelected { get; set; }
    }
}