using System;
using System.Reflection;
using System.Web;

namespace NRack.Hosting.AspNet
{
    public class RackHttpApplication : HttpApplication
    {
        public Assembly CallingAssembly { get; set; }
        protected void Application_Start(object sender, EventArgs e)
        {
            CallingAssembly = Assembly.GetCallingAssembly();
        }
    }
}