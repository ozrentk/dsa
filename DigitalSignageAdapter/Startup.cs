using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DigitalSignageAdapter.Startup))]
namespace DigitalSignageAdapter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
