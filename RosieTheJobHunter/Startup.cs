using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RosieTheJobHunter.Startup))]
namespace RosieTheJobHunter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
