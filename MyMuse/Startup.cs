using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyMuse.Startup))]
namespace MyMuse
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
