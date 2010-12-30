using System;
using System.Web;
using NRack.Auth;
using NRack.Configuration;

namespace NRack.Example
{
    public class RackConfig : RackConfigBase
    {
        public override void RackUp()
        {
            Use<BasicAuthHandler>("Lobster", 
                (Func<string, string, bool>)((username, password) => password == "secret"))
            .Map("/app", 
                rack =>
                    rack.Use<YuiCompressor>(HttpContext.Current.Request.MapPath("~/"))
                        .Run(new MyApp()))
            .Map("/env", rack => rack.Run(new EnvironmentOutput()));
        }
    }
}