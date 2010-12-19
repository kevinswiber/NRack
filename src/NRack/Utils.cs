using System.Collections.Specialized;
using System.Web;

namespace NRack
{
    public class Utils
    {
        public string Escape(string uri)
        {
            return HttpUtility.UrlEncode(uri);
        }

        public string Unescape(string uri)
        {
            return HttpUtility.UrlDecode(uri);
        }

        public NameValueCollection ParseQuery(string queryString)
        {
            return HttpUtility.ParseQueryString(queryString);
        }
    }
}