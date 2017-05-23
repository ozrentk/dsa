using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DSA.WEB.Models.Identity
{
    public class DsaAppUser : IdentityUser<int, DsaAppUserLogin, DsaAppUserRole, DsaAppUserClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<DsaAppUser, int> manager)
        {
            // NOTE: authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here, if needed

            return userIdentity;
        }
    }
}