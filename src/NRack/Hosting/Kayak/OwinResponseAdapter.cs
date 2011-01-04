using System.Collections.Generic;
using System.Text;
using NRack.Adapters;

namespace NRack.Hosting.Kayak
{
    public class OwinResponseAdapter
    {
        private readonly dynamic[] _response;

        public OwinResponseAdapter(dynamic[] response)
        {
            _response = response;
        }

        public IEnumerable<object> GetBody()
        {
            string body = _response[2].ToString();
            return new[] {Encoding.ASCII.GetBytes(body)};
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

        public IDictionary<string, IList<string>> Headers
        {
            get
            {
                var adapted = new Dictionary<string, IList<string>>();
                var headers = new HeaderHash(_response[1]);

                headers.Each(pair => adapted.Add(pair.Key, new string[] {pair.Value.ToString()}));

                return adapted;
            }
        }
    }
}