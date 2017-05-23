using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSA.WEB.Models.Identity
{
    public class DsaAppRole : IdentityRole<int, DsaAppUserRole>
    {
        public DsaAppRole() { }

        public DsaAppRole(string name)
        {
            Name = name;
        }
    }
}