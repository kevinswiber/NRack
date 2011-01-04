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
            get { return _response[0].ToString(); }
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