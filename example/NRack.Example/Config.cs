using System;
using System.Web;
using NRack.Adapters;
using NRack.Auth;
using NRack.Configuration;

namespace NRack.Example
{
    public class Config : ConfigBase
    {
        /// <summary>
        /// Slightly complex example.
        /// </summary>
        public override void RackUp()
        {
            Use<BasicAuthHandler>("Rack Lobster!",
                                  (Func<string, string, bool>)
                                  ((username, password) => password == "p4ssw0rd!"))
                .Map("/", rack =>
                          rack.Run(env =>
                                   new dynamic[] {200, new Hash{{"Content-Type", "text/html"}},
                                       "<h1>Hello, World!</h1>"}))
                .Map("/app", rack =>
                             rack.Map("/scripts", scripts =>
                                 scripts.Run(new YuiCompressor(null, HttpContext.Current.Request.MapPath("~/Scripts/"))))
                             .Map("/", appRoot => appRoot.Run(new MyApp())))
                .Map("/env", rack =>
                             rack.Run(new EnvironmentOutput()));
        }

        ///// <summary>
        ///// A simple RackUp example.
        ///// </summary>
        //public override void RackUp()
        //{
        //    Run(environment =>
        //            new dynamic[] { 200, new Hash {{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>" });
        //}
    }
}