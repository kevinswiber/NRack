using System;
using Kayak;

namespace NRack.Example.Kayak
{
    class SchedulerDelegate : ISchedulerDelegate
    {
        public void OnException(IScheduler scheduler, Exception e)
        {
            Console.WriteLine(e);
        }

        public void OnStop(IScheduler scheduler)
        {
            Console.WriteLine("Stopped");
        }
    }
}