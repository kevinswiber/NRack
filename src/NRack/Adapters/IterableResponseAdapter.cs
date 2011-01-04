using System.Collections.Generic;

namespace NRack.Adapters
{
    public class IterableResponseAdapter : ICallable
    {
        private readonly dynamic _innerObject;

        public IterableResponseAdapter(dynamic innerObject)
        {
            _innerObject = innerObject;
        }
        
        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var response = _innerObject.Call(environment);

            response[2] = new IterableAdapter(response[2]);

            return response;
        }
    }
}