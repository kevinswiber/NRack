using System.Collections.Generic;

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
            get { return Environment["SERVER_PORT"]; }
        }

        public string RequestMethod
        {
            get { return Environment["REQUEST_METHOD"]; }
        }

        public string QueryString
        {
            get { return Environment["QUERY_STRING"]; }
        }

        public string ContentLength
        {
            get { return Environment["CONTENT_LENGTH"]; }
        }

        public string ContentType
        {
            get { return Environment["CONTENT_TYPE"]; }
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
    }
}