using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models
{
    public class Config_old
    {
        public ConfigItems ConfigItems { get; set; }
        public List<ConfigLineBusiness> ConfigLineBusinessItems { get; set; }

        public bool AddLineBusiness { get; set; }
        public bool RemoveLineBusinesses { get; set; }
        public ConfigLineBusiness NewLineBusiness { get; set; }
        public bool SaveConfig { get; set; }
    }
}