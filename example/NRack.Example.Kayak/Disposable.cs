using System;

namespace NRack.Example.Kayak
{
    class Disposable : IDisposable
    {
        readonly Action dispose;

        public Disposable(Action dispose)
        {
            this.dispose = dispose;
        }

        public void Dispose()
        {
            dispose();
        }
    }
}