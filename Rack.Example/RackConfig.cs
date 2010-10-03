using Rack.Hosting.AspNet;

namespace Rack.Example
{
    public class RackConfig : RackConfigBase
    {
        public override void RackUp()
        {
            //Use<EnvironmentOutput>();
            //Use<JavaScriptMinifier>("./");
            Map("/app", rack =>
                            {
                                rack.Use<JavaScriptMinifier>("./");
                                rack.Run(new MyApp());
                            });
            Map("/env", rack => rack.Run(new EnvironmentOutput(null)));
        }
    }
}