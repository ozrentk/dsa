using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalSignageAdapter.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public string LastTicketNumber { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationUserRole : IdentityUserRole<int>
    {
    }

    public class ApplicationUserClaim : IdentityUserClaim<int>
    {
    }

    public class ApplicationUserLogin : IdentityUserLogin<int>
    {
    }

    public class ApplicationRole : IdentityRole<int, ApplicationUserRole>
    {
        public ApplicationRole() { }
        public ApplicationRole(string name) { Name = name; }
    }

    public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public ApplicationUserStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class CustomRoleStore : RoleStore<ApplicationRole, int, ApplicationUserRole>
    {
        public CustomRoleStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim> 

    {
        public ApplicationDbContext()
            : base("DsaConnection")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var user = modelBuilder.Entity<ApplicationUser>()
                .ToTable("User", "SEC")
                .HasKey(u => u.Id);
            //user.Property(u => u.Id).HasColumnName("Id");
            user.HasMany(u => u.Claims).WithOptional().HasForeignKey(c => c.UserId);
            user.HasMany(u => u.Logins).WithOptional().HasForeignKey(l => l.UserId);
            user.HasMany(u => u.Roles).WithOptional().HasForeignKey(r => r.UserId);

            var role = modelBuilder.Entity<ApplicationRole>()
                .ToTable("Roles", "SEC")
                .HasKey(r => r.Id);
            //user.Property(u => u.Id).HasColumnName("Id");
            role.HasMany(u => u.Users).WithOptional().HasForeignKey(u => u.UserId);

            var usersRole = modelBuilder.Entity<ApplicationUserRole>()
                .ToTable("UserRoles", "SEC")
                .HasKey(ur => new { ur.RoleId, ur.UserId });
            //usersRole.Property(ur => ur.RoleId).HasColumnName("RoleId");
            //usersRole.Property(ur => ur.UserId).HasColumnName("UserId");

            var claim = modelBuilder.Entity<ApplicationUserClaim>()
                .ToTable("UserClaims", "SEC")
                .HasKey(uc => uc.Id);
            //claim.Property(uc => uc.UserId).HasColumnName("UserId");

            var login = modelBuilder.Entity<ApplicationUserLogin>()
                .ToTable("UserLogins", "SEC")
                .HasKey(ul => new { ul.LoginProvider, ul.ProviderKey, ul.UserId });
            //login.Property(ul => ul.UserId).HasColumnName("UserId");

            Database.SetInitializer<ApplicationDbContext>(null);
        }
    }
}