using Rack.Hosting.AspNet;

namespace RackExample
{
    public class RackConfig : RackConfigBase
    {
        public override void RackUp()
        {
            Use<EnvironmentOutput>();
            //Use<JavaScriptMinifier>("./");
            Map("/app", builder =>
                            {
                                builder.Use<JavaScriptMinifier>("./");
                                builder.Run(new MyApp());
                            });
        }
    }
}