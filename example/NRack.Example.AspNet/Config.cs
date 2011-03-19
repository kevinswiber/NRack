using System;
using NRack.Auth;
using NRack.Configuration;
using NRack.Helpers;

namespace NRack.Example.AspNet
{
    public class Config : ConfigBase
    {
        /// <summary>
        /// Slightly complex example.
        /// </summary>
        public override void Start()
        {
            Use<BasicAuthHandler>("Rack Lobster!",
                                  (Func<string, string, bool>)
                                  ((username, password) => password == "p4ssw0rd!"))
                .Map("/assets", rack =>
                                rack.Run(new File(AppDomain.CurrentDomain.BaseDirectory + @"Files\")))
                .Map("/", rack =>
                          rack.Run(env =>
                                   new dynamic[] {200, new Hash{{"Content-Type", "text/html"}},
                                       "<h1>Hello, World!</h1>"}))
                .Map("/app", rack =>
                             rack.Map("/scripts", scripts =>
                                 scripts.Run(new YuiCompressor(AppDomain.CurrentDomain.BaseDirectory + @"Scripts\")))
                             .Map("/", appRoot => appRoot.Run(new MyApp())))
                .Map("/env", rack =>
                             rack.Run(new EnvironmentOutput()));
        }

        ///// <summary>
        ///// A simple config example.
        ///// </summary>
        //public override void Start()
        //{
        //    Run(environment =>
        //            new dynamic[] { 200, new Hash {{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>" });
        //}
    }
}