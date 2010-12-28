using System.Web;
using NRack.Configuration;

namespace NRack.Example
{
    public class RackConfig : RackConfigBase
    {
        public override void RackUp()
        {
            Map("/app", 
                rack =>
                    rack.Use<YuiCompressor>(HttpContext.Current.Request.MapPath("~/"))
                        .Run(new MyApp()));

            Map("/env", rack => rack.Run(new EnvironmentOutput()));
        }
    }
}