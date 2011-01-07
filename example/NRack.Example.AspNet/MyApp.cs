using System.Collections.Generic;
using NRack.Helpers;

namespace NRack.Example.AspNet
{
    public class MyApp : ICallable
    {
        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            return new dynamic[] { 200, new Hash{{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
        }
    }
}