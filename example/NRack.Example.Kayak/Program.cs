using System;
using System.Collections.Generic;
using NRack.Adapters;

namespace NRack.Example.Kayak
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.Start(new Hash {{ServerOptions.App, GetBuilderFunc()}});
        }

        static Proc GetBuilderFunc()
        {
            var config = new Config();
            Func<IDictionary<string, dynamic>, dynamic> builderInContextFunc = 
                env => new Builder(config.ExecuteRackUp).Call(env);

            return new Proc(builderInContextFunc);
        }
    }
}
