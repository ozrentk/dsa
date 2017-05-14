using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models
{
    public class ConfigLineBusiness
    {
        public int LineId { get; set; }
        public int BusinessId { get; set; }
        public bool Remove { get; set; }
    }
}