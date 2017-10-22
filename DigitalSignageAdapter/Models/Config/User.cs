using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.Models.Config
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Role> Roles { get; set; }
    }
}