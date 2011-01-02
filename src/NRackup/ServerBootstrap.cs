using System;
using System.Collections.Generic;
using NRack;
using NRack.Adapters;

namespace NRackup
{
    public static class ServerBootstrap
    {
        public static void Start()
        {
            var options = new Hash { { ServerOptions.App, GetBuilderFunc() } };

            Server.Start(options);
        }

        static Proc GetBuilderFunc()
        {
            var config = ConfigFactory.NewInstance();
            Func<IDictionary<string, object>, object> builderInContextFunc =
                env => new Builder(config.ExecuteRackUp).Call(env);

            return new Proc(builderInContextFunc);
        }
    }
}