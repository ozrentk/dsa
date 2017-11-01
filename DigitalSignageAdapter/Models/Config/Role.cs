using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class Role
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public List<Permission> Permissions { get; set; }
        public bool IsDelete { get; set; }
        public bool IsSelected { get; set; }
    }
}