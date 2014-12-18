using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ChampsRoom.Startup))]
namespace ChampsRoom
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
