using System;
using System.Collections.Generic;
using System.Net;
using Gate;
using Gate.Kayak;
using NRack.Hosting.Owin;

namespace NRack.Example.Kayak
{
    class Program
    {
        static void Main(string[] args)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 8080);

            Console.WriteLine("Running... {0}.", endPoint);

            KayakGate.Start(new SchedulerDelegate(), endPoint, Startup.Configuration);

            Console.ReadLine();
        }
    }

    public class Startup
    {
        public static void Configuration(IAppBuilder appBuilder)
        {
            var handler = new OwinHandler();
            appBuilder
                .RescheduleCallbacks()
                .Run(Delegates.ToDelegate(handler.ProcessRequest));
        }
    }
}
