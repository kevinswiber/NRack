using NRack.Adapters;
using NRack.Configuration;

namespace NRack.Example.Kayak
{
    public class Config : ConfigBase
    {
        #region Overrides of ConfigBase

        public override void RackUp()
        {
            var calledCount = ++_calledCount;
            Run(env =>
                    {
                        System.Diagnostics.Debug.WriteLine("RackUp Thread #" + calledCount + ": " + System.Threading.Thread.CurrentThread.ManagedThreadId);
                        System.Threading.Thread.Sleep(3000);
                        return new dynamic[] {200, new Hash {{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
                    }
                    );
        }

        #endregion

        private static int _calledCount = 0;
    }
}