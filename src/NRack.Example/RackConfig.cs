using System.Web;

namespace NRack.Example
{
    public class RackConfig : RackConfigBase
    {
        public override void RackUp()
        {
            //Use<EnvironmentOutput>();
            //Use<YuiCompressor>(HttpContext.Current.Request.MapPath("~/"));
            Map("/app", rack =>
                            {
                                rack.Use<YuiCompressor>(HttpContext.Current.Request.MapPath("~/"));
                                rack.Run(new MyApp());
                            });
            Map("/env", rack => rack.Run(new EnvironmentOutput()));
        }
    }
}