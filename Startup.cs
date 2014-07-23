using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BuggerOff.Startup))]
namespace BuggerOff
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
