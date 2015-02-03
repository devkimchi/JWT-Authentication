using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JwtAuthentication.WebApp.Startup))]
namespace JwtAuthentication.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
