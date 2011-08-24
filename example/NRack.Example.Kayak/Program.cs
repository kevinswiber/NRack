using System.Net;
using System.Text;
using Kayak;
using Kayak.Http;

namespace NRack.Example.Kayak
{
    class Program
    {
        static void Main(string[] args)
        {
            var scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
            var server = KayakServer.Factory.CreateHttp(new RequestDelegate(), scheduler);

            using (server.Listen(new IPEndPoint(IPAddress.Any, 8080)))
            {
                // runs scheduler on calling thread. this method will block until
                // someone calls Stop() on the scheduler.
                scheduler.Start();
            }
        }
    }
}
