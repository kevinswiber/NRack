using NRack.Adapters;
using NRack.Configuration;

namespace NRack.Example
{
    public class Config : ConfigBase
    {
        /// <summary>
        /// A simple RackUp example.
        /// </summary>
        public override void RackUp()
        {
            Run(environment =>
                    new dynamic[] { 200, new Hash {{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>" });
        }

        ///// <summary>
        ///// More complex example.
        ///// </summary>
        //public override void RackUp()
        //{
        //    Use<BasicAuthHandler>("Lobster",
        //        (Func<string, string, bool>)((username, password) => password == "secret"))
        //    .Map("/app",
        //        rack =>
        //            rack.Use<YuiCompressor>(HttpContext.Current.Request.MapPath("~/"))
        //                .Run(new MyApp()))
        //    .Map("/env", rack => rack.Run(new EnvironmentOutput()));
        //}
    }
}