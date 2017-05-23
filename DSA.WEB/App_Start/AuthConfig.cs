using Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using DSA.WEB.Models.Identity;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Owin.Security;
using System.Security.Claims;
using NLog;

//[assembly: OwinStartup(typeof(SignalRChat.Startup))]
//namespace SignalRChat
//{
//    public class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            // Any connection or hub wire up and configuration should go here
//            app.MapSignalR();
//        }
//    }
//}

namespace DSA.WEB
{
    public static class AuthConfig
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Configure(IAppBuilder app)
        {
            logger.Debug("Starting configure...");
            try
            {
                // Configure the db context, user manager and signin manager to use a single instance per request
                app.CreatePerOwinContext(DsaAppDbContext.Create);
                app.CreatePerOwinContext<DsaAppUserManager>(DsaAppUserManager.Create);
                app.CreatePerOwinContext<DsaAppSignInManager>(DsaAppSignInManager.Create);
                logger.Debug("DB context configured");

                // Enable the application to use a cookie to store information for the signed in user
                // and to use a cookie to temporarily store information about a user logging in with a third party login provider
                // Configure the sign in cookie
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                    LoginPath = new PathString("/Authenticate/Login"),
                    Provider = new CookieAuthenticationProvider
                    {
                        // Enables the application to validate the security stamp when the user logs in.
                        // This is a security feature which is used when you change a password or add an external login to your account.  
                        OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<DsaAppUserManager, DsaAppUser, int>(
                            validateInterval: TimeSpan.FromMinutes(30),
                            //regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)
                            regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager),
                            getUserIdCallback: (id) => (id.GetUserId<int>()))
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }

    public class DsaAppUserManager : UserManager<DsaAppUser, int>
    {
        public DsaAppUserManager(IUserStore<DsaAppUser, int> store)
            : base(store)
        {
        }

        public static DsaAppUserManager Create(IdentityFactoryOptions<DsaAppUserManager> options, IOwinContext context)
        {
            var manager = new DsaAppUserManager(new DsaAppUserStore(context.Get<DsaAppDbContext>()));

            // Configure validation logic for usernames
            // NOTE: only alphanum. usernames, and unique email is needed
            manager.UserValidator = new UserValidator<DsaAppUser, int>(manager);

            // Configure validation logic for passwords
            // NOTE: this allows for retarded passwords like "godgodgod" etc.
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8
            };

            // NOTE: we don't care about user lockout; actually, please, *DO NOT use lockout*
            manager.UserLockoutEnabledByDefault = false;

            // This should work for validate user and forgot pwd...
            manager.EmailService = new EmailService();

            // NOTE: we *DON'T CARE about two factor authentication*

            // This is for email confirmation token or a password reset token
            // SEE: http://stackoverflow.com/questions/25685252/dataprotectionprovider-in-the-identity-sample-project
            // SEE: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/introduction
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<DsaAppUser, int>(
                        dataProtectionProvider.Create("ASP.NET Identity"));
            }

            return manager;
        }
    }

    public class DsaAppSignInManager : SignInManager<DsaAppUser, int>
    {
        public DsaAppSignInManager(DsaAppUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(DsaAppUser user)
        {
            return user.GenerateUserIdentityAsync((DsaAppUserManager)UserManager);
        }

        public static DsaAppSignInManager Create(IdentityFactoryOptions<DsaAppSignInManager> options, IOwinContext context)
        {
            return new DsaAppSignInManager(context.GetUserManager<DsaAppUserManager>(), context.Authentication);
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            sendMail(message);
            return Task.FromResult(0);
        }

        void sendMail(IdentityMessage message)
        {
            var mailMessage = new MailMessage("ozren.t.k@hotmail.com", message.Destination);
            mailMessage.From = new MailAddress("ozren.t.k@hotmail.com", "Don't reply");
            mailMessage.Subject = message.Subject;
            mailMessage.Body = message.Body;

            var client = new SmtpClient();
            client.EnableSsl = true;
            client.Send(mailMessage);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

}

