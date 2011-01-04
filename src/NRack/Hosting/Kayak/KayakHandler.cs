using System;
using System.Net;
using Kayak;
using NRack.Adapters;
using NRack.ServerHelpers;

namespace NRack.Hosting.Kayak
{
    public class KayakHandler
    {
        public void Run(dynamic app, Hash options)
        {
            var host = options[ServerOptionKeys.Host].ToString();
            var port = Convert.ToInt32(options[ServerOptionKeys.Port]);

            var server =
                new DotNetServer(new IPEndPoint((Dns.Resolve(host)).AddressList[0], port));

            var pipe = server.Start();

            server.Host((env, respond, error) =>
                            {
                                var response = app.Call(env);
                                var adapter = new OwinResponseAdapter(response);
                                respond(adapter.Status, adapter.Headers, adapter.GetBody());
                            });

            Console.CancelKeyPress += (obj, args) => Cleanup(pipe);

            Console.WriteLine("Kayak is running at http://" + server.ListenEndPoint.Address + ":" +
                              server.ListenEndPoint.Port);
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            Cleanup(pipe);
        }

        private void Cleanup(IDisposable pipe)
        {
            if (pipe != null)
            {
                pipe.Dispose();
            }
        }
    }
}