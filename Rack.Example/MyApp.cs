using System.Collections.Generic;
using Rack;

namespace RackExample
{
    public class MyApp
    {
        public dynamic[] Call(IDictionary<string, string> environment)
        {
            return new dynamic[] { 200, new Headers{{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
        }
    }
}