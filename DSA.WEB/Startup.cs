using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DSA.WEB.OwinStartup))]
namespace DSA.WEB
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            AuthConfig.Configure(app);
        }
    }
}
