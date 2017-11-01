using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace DigitalSignageAdapter.Models
{
    //public class PasswordOptions
    //{
    //    public bool RequireNonLetterOrDigit { get; set; }
    //    public bool RequireDigit { get; set; }
    //    public bool RequireLowercase { get; set; }
    //    public bool RequireUppercase { get; set; }
    //    public int RequiredLength { get; set; }
    //}

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public bool IsActive { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public static string GeneratePassword(Microsoft.AspNet.Identity.PasswordValidator validator)
        {
            if (validator == null)
                throw new ArgumentException("Missing parameter: ", nameof(validator));

            bool requireNonLetterOrDigit = validator.RequireNonLetterOrDigit;
            bool requireDigit = validator.RequireDigit;
            bool requireLowercase = validator.RequireLowercase;
            bool requireUppercase = validator.RequireUppercase;

            string randomPassword = string.Empty;

            int passwordLength = validator.RequiredLength;

            Random random = new Random();
            while (randomPassword.Length != passwordLength)
            {
                int randomNumber = random.Next(48, 122);  // >= 48 && < 122 
                if (randomNumber == 95 || randomNumber == 96) continue;  // != 95, 96 _'

                char c = Convert.ToChar(randomNumber);

                if (requireDigit)
                    if (char.IsDigit(c))
                        requireDigit = false;

                if (requireLowercase)
                    if (char.IsLower(c))
                        requireLowercase = false;

                if (requireUppercase)
                    if (char.IsUpper(c))
                        requireUppercase = false;

                if (requireNonLetterOrDigit)
                    if (!char.IsLetterOrDigit(c))
                        requireNonLetterOrDigit = false;

                randomPassword += c;
            }

            Action<int, int> jokerCharInsert =
                (fromAscii, toAscii) =>
                {
                    var pos = random.Next(0, randomPassword.Length - 1);
                    var chr = Convert.ToChar(random.Next(fromAscii, toAscii)).ToString();
                    randomPassword = randomPassword.Insert(pos, chr);
                };

            if (requireDigit)
            {
                jokerCharInsert(48, 58);
            }

            if (requireLowercase)
            {
                jokerCharInsert(97, 123);
            }

            if (requireUppercase)
            {
                jokerCharInsert(65, 91);
            }

            if (requireNonLetterOrDigit)
            {
                jokerCharInsert(33, 48);
            }

            return randomPassword;
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