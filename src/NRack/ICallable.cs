using System.Collections.Generic;

namespace NRack
{
    public interface ICallable
    {
        dynamic[] Call(IDictionary<string, dynamic> environment);
    }
}