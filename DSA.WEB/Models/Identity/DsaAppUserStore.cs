using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSA.WEB.Models.Identity
{
    public class DsaAppUserStore : UserStore<DsaAppUser, DsaAppRole, int, DsaAppUserLogin, DsaAppUserRole, DsaAppUserClaim>
    {
        public DsaAppUserStore(DsaAppDbContext context)
            : base(context)
        {
        }
    }
}