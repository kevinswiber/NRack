using Rack.Hosting.AspNet;

namespace Rack.Example
{
    public class RackConfig : RackConfigBase
    {
        public override void RackUp()
        {
            //Use<EnvironmentOutput>();
            //Use<JavaScriptMinifier>("./");
            Map("/app", builder =>
                            {
                                builder.Use<JavaScriptMinifier>("./");
                                builder.Run(new MyApp());
                            });
            Map("/env", builder => builder.Run(new EnvironmentOutput(null)));
        }
    }
}