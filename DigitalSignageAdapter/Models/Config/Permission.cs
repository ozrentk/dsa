using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class Permission
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }
    }
}