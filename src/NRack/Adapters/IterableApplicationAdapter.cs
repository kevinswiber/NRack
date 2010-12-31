using System.Collections.Generic;

namespace NRack.Adapters
{
    public class IterableApplicationAdapter : IApplication
    {
        private readonly dynamic _innerObject;

        public IterableApplicationAdapter(dynamic innerObject)
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