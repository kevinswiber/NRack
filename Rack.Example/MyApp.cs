using System.Collections.Generic;

namespace Rack.Example
{
    public class MyApp
    {
        public dynamic[] Call(IDictionary<string, string> environment)
        {
            return new dynamic[] { 200, new Headers{{"Content-Type", "text/html"}}, "<h1>Hello, World!</h1>"};
        }
    }
}