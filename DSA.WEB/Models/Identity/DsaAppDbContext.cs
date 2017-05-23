using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DSA.WEB.Models.Identity
{
    public class DsaAppDbContext : IdentityDbContext<DsaAppUser, DsaAppRole, int, DsaAppUserLogin, DsaAppUserRole, DsaAppUserClaim>

    {
        public DsaAppDbContext() : base("AdapterDbEntities")
        {
        }

        public static DsaAppDbContext Create()
        {
            return new DsaAppDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var user = modelBuilder.Entity<DsaAppUser>()
                .ToTable("User", "SEC")
                .HasKey(u => u.Id);
            user.HasMany(u => u.Claims).WithOptional().HasForeignKey(c => c.UserId);
            user.HasMany(u => u.Logins).WithOptional().HasForeignKey(l => l.UserId);
            user.HasMany(u => u.Roles).WithOptional().HasForeignKey(r => r.UserId);

            var role = modelBuilder.Entity<DsaAppRole>()
                .ToTable("Roles", "SEC")
                .HasKey(r => r.Id);
            role.HasMany(u => u.Users).WithOptional().HasForeignKey(u => u.UserId);

            var usersRole = modelBuilder.Entity<DsaAppUserRole>()
                .ToTable("UserRoles", "SEC")
                .HasKey(ur => new { ur.RoleId, ur.UserId });

            var claim = modelBuilder.Entity<DsaAppUserClaim>()
                .ToTable("UserClaims", "SEC")
                .HasKey(uc => uc.Id);

            var login = modelBuilder.Entity<DsaAppUserLogin>()
                .ToTable("UserLogins", "SEC")
                .HasKey(ul => new { ul.LoginProvider, ul.ProviderKey, ul.UserId });

            Database.SetInitializer<DsaAppDbContext>(null);
        }
    }

}