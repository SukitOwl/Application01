using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Application01.Startup))]
namespace Application01
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
