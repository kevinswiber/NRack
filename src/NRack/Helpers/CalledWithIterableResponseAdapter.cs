using System.Collections.Generic;

namespace NRack.Helpers
{
    public class CalledWithIterableResponseAdapter : ICallable
    {
        private readonly dynamic _innerObject;

        public CalledWithIterableResponseAdapter(dynamic innerObject)
        {
            _innerObject = innerObject;
        }
        
        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var response = _innerObject.Call(environment);

            if (response.Length > 2 && response[2] != null)
            {
                response[2] = new IterableAdapter(response[2]);
            }

            return response;
        }
    }
}