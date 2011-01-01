using System.Collections.Generic;
using NRack.Adapters;

namespace NRack.Example.AspNet
{
    public class MyApp : IApplication
    {
        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            return new dynamic[] { 200, new Hash{{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
        }
    }
}