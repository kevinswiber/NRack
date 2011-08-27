using System;
using System.Collections.Generic;
using System.Text;
using NRack.Helpers;

namespace NRack.Hosting.Owin
{
    public class OwinResponseAdapter
    {
        private readonly dynamic[] _response;

        public OwinResponseAdapter(dynamic[] response)
        {
            _response = response;
        }

        public ArraySegment<byte> GetBody()
        {
            dynamic body = _response[2];

            var bodyBytes = new List<byte>();

            body.Each((Action<dynamic>)(innerBody =>
                                            {
                                                if (innerBody is string)
                                                {
                                                    bodyBytes.AddRange(Encoding.ASCII.GetBytes(innerBody));
                                                }
                                                else
                                                {
                                                    bodyBytes.AddRange(innerBody);
                                                }
                                            }));

            return new ArraySegment<byte>(bodyBytes.ToArray());

            //return new[] {Encoding.ASCII.GetBytes(body)};
        }

        public string Status
        {
            get
            {
                var statusCode = _response[0];
                var status = statusCode;

                if (statusCode is int && Utils.HttpStatusCodes.ContainsKey(statusCode))
                {
                    status += " " + Utils.HttpStatusCodes[statusCode];
                }

                return status;
            }
        }

        public IDictionary<string, string> Headers
        {
            get
            {
                var adapted = new Dictionary<string, string>();
                var headers = new HeaderHash(_response[1]);

                headers.Each(pair => adapted.Add(pair.Key, pair.Value.ToString()));

                return adapted;
            }
        }
    }
}