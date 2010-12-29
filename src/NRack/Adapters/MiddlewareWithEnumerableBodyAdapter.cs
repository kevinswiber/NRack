using System.Collections.Generic;

namespace NRack.Adapters
{
    public class MiddlewareWithEnumerableBodyAdapter : IApplication
    {
        private readonly dynamic _innerObject;

        public MiddlewareWithEnumerableBodyAdapter(dynamic innerObject)
        {
            _innerObject = innerObject;
        }
        
        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var response = _innerObject.Call(environment);

            response[2] = new EnumerableBodyAdapter(response[2]);

            return response;
        }
    }
}