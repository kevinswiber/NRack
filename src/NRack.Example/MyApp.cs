using System.Collections.Generic;
using NRack.Adapters;

namespace NRack.Example
{
    public class MyApp : IApplication
    {
        public dynamic[] Call(IDictionary<string, object> environment)
        {
            return new dynamic[] { 200, new Headers{{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
        }
    }
}