using System.Collections.Generic;

namespace Rack.Hosting.AspNet
{
    public class MiddlewareWithEnumerableBodyAdapter
    {
        private readonly dynamic _innerObject;

        public MiddlewareWithEnumerableBodyAdapter(dynamic innerObject)
        {
            _innerObject = innerObject;
        }
        
        public dynamic[] Call(IDictionary<string, string> environment)
        {
            var response = _innerObject.Call(environment);

            response[2] = new EnumerableBodyAdapter(response[2]);

            return response;
        }
    }
}