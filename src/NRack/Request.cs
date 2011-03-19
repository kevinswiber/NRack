using System;
using System.Collections.Generic;
using System.Linq;

namespace NRack
{
    public class Request
    {
        public IDictionary<string, dynamic> Environment { get; private set; }

        public Request(IDictionary<string, dynamic> env)
        {
            Environment = env;
        }

        public dynamic Body
        {
            get { return Environment["rack.input"]; }
        }

        public string Scheme
        {
            get { return Environment["rack.url_scheme"]; }
        }

        public string ScriptName
        {
            get { return Environment["SCRIPT_NAME"]; }
        }

        public string PathInfo
        {
            get { return Environment["PATH_INFO"]; }
        }

        public int Port
        {
            get { return Convert.ToInt32(Environment["SERVER_PORT"]); }
        }

        public string RequestMethod
        {
            get { return Environment["REQUEST_METHOD"]; }
        }

        public string QueryString
        {
            get { return Environment["QUERY_STRING"]; }
        }

        public int ContentLength
        {
            get { return Convert.ToInt32(Environment["CONTENT_LENGTH"]); }
        }

        public string ContentType
        {
            get { return Environment["CONTENT_TYPE"]; }
        }

        public bool IsDelete { get { return RequestMethod == "DELETE"; } }
        public bool IsGet { get { return RequestMethod == "GET"; } }
        public bool IsHead { get { return RequestMethod == "HEAD"; } }
        public bool IsPost { get { return RequestMethod == "POST"; } }
        public bool IsPut { get { return RequestMethod == "PUT"; } }
        public bool IsTrace { get { return RequestMethod == "TRACE"; } }

        public string HostWithPort
        {
            get
            {
                if (Environment.ContainsKey("HTTP_X_FORWARDED_HOST"))
                {
                    var forwarded = (string)Environment["HTTP_X_FORWARDED_HOST"];
                    return forwarded.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
                }

                if (Environment.ContainsKey("HTTP_HOST"))
                {
                    return Environment["HTTP_HOST"];
                }

                string hostWithPort = Environment.ContainsKey("SERVER_NAME")
                    ? Environment["SERVER_NAME"]
                    : Environment["SERVER_ADDR"];

                hostWithPort += ":" + Environment["SERVER_PORT"];

                return hostWithPort;
            }
        }

        public string Host
        {
            get
            {
                return HostWithPort.Contains(":")
                    ? HostWithPort.Substring(0, HostWithPort.IndexOf(":"))
                    : HostWithPort;
            }
        }

        public dynamic Session
        {
            get
            {
                if (!Environment.ContainsKey("rack.session"))
                {
                    Environment["rack.session"] = new Dictionary<string, dynamic>();
                }

                return Environment["rack.session"];
            }
        }

        public dynamic SessionOptions
        {
            get
            {
                if (!Environment.ContainsKey("rack.session_options"))
                {
                    Environment["rack.session_options"] = new Dictionary<string, dynamic>();
                }


                return Environment["rack.session_options"];
            }
        }

        public dynamic Logger
        {
            get { return Environment["rack.logger"]; }
        }

        public IDictionary<string, string> GET
        {
            get
            {
                const string QueryStringKey = "rack.request.query_string";
                const string QueryHashKey = "rack.request.query_hash";

                if (Environment.ContainsKey(QueryStringKey) && Environment[QueryStringKey] == QueryString)
                {
                    return Environment[QueryHashKey];
                }

                Environment[QueryStringKey] = QueryString;
                Environment[QueryHashKey] = ParseQuery(QueryString);
                return Environment[QueryHashKey];
            }
        }

        public IDictionary<string, string> POST
        {
            get { return new Dictionary<string, string>(); }
        }

        public IDictionary<string, string> Params
        {
            get
            {
                return GET.Concat(POST).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        protected IDictionary<string, string> ParseQuery(string queryString)
        {
            return Utils.ParseNestedQuery(queryString);
        }
    }
}