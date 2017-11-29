using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PersonalAccount.Startup))]
namespace PersonalAccount
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
