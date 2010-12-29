using System;
using System.Collections.Specialized;
using NRack.Adapters;

namespace NRack.Hosting.AspNet
{
    public class AspNetResponse
    {
        public int StatusCode { get; private set; }
        public NameValueCollection Headers { get; private set; }
        public dynamic Body { get; private set; }

        public static AspNetResponse Create(dynamic[] parameters)
        {
            var response = new AspNetResponse
                               {
                                   StatusCode = GetStatusCode(parameters),
                                   Headers = GetHeaders(parameters),
                                   Body = GetBody(parameters)
                               };

            return response;
        }

        private static dynamic GetBody(dynamic[] parameters)
        {
            if (parameters.Length < 3)
            {
                return null;
            }
            var body = parameters[2];

            if (body is string)
            {
                return body;
            }

            return body;
        }

        private static int GetStatusCode(dynamic[] parameters)
        {
            if (parameters.Length < 1)
            {
                return 0;
            }

            var statusCode = parameters[0];
            if (statusCode == null)
            {
                return 0;
            }

            if (statusCode is int)
            {
                return statusCode;
            }

            return Convert.ToInt32(statusCode);
        }
        
        private static NameValueCollection GetHeaders(dynamic[] parameters)
        {
            Headers headers = parameters.Length < 2 ? null : parameters[1];

            var nvCollection = new NameValueCollection();

            if (headers != null)
            {
                headers.Each(pair => nvCollection.Add(pair.Key, pair.Value));
            }

            return nvCollection;
        }
    }
}