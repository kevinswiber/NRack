using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NRack
{
    public class Request
    {
        public static readonly string[] FormDataMediaTypes =
            new[] { "application/x-www-form-urlencoded", "multipart/formdata" };

        public static readonly string[] ParseableDataMediaTypes =
            new[] { "multipart/related", "multipart/mixed" };

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
            get
            {
                if (Environment.ContainsKey("HTTPS") && Environment["HTTPS"] == "on")
                {
                    return "https";
                }

                if (Environment.ContainsKey("HTTP_X_FORWARDED_SSL") && Environment["HTTP_X_FORWARDED_SSL"] == "on")
                {
                    return "https";
                }

                if (Environment.ContainsKey("HTTP_X_FORWARDED_PROTO") && Environment["HTTP_X_FORWARDED_PROTO"] != null)
                {
                    return Environment["HTTP_X_FORWARDED_PROTO"].Split(',')[0];
                }

                return Environment["rack.url_scheme"];
            }
        }

        public bool IsSsl
        {
            get { return Scheme.ToLower() == "https"; }
        }

        public string ScriptName
        {
            get { return Environment["SCRIPT_NAME"]; }
            set { Environment["SCRIPT_NAME"] = value; }
        }

        public string PathInfo
        {
            get { return Environment["PATH_INFO"]; }
            set { Environment["PATH_INFO"] = value; }
        }

        public int Port
        {
            get
            {
                if (HostWithPort.Contains(":"))
                {
                    return Convert.ToInt32(HostWithPort.Split(':')[1]);
                }

                if (Environment.ContainsKey("HTTP_X_FORWARDED_PORT") && Environment["HTTP_X_FORWARDED_PORT"] != null)
                {
                    return Convert.ToInt32(Environment["HTTP_X_FORWARDED_PORT"]);
                }

                if (IsSsl)
                {
                    return 443;
                }

                if (Environment.ContainsKey("HTTP_X_FORWARDED_HOST"))
                {
                    return 80;
                }

                return Convert.ToInt32(Environment["SERVER_PORT"]);
            }
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
        public bool IsPatch { get { return RequestMethod == "PATCH"; } }

        public bool HasFormData
        {
            get
            {
                var type = MediaType;
                var method = Environment.ContainsKey("rack.methodoverride.original_method")
                                 ? Environment["rack.methodoverride.original_method"]
                                 : Environment["REQUEST_METHOD"];

                return (method == "POST" && type == null) || FormDataMediaTypes.Contains(type);
            }
        }

        public bool HasParseableData
        {
            get { return ParseableDataMediaTypes.Contains(MediaType); }
        }

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
                    : (Environment.ContainsKey("SERVER_ADDR") ? Environment["SERVER_ADDR"] : string.Empty);

                if (hostWithPort != string.Empty)
                {
                    hostWithPort += ":" + Environment["SERVER_PORT"];
                }

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

        public string MediaType
        {
            get
            {
                if (string.IsNullOrEmpty(ContentType))
                {
                    return null;
                }

                return Regex.Split(ContentType, @"\s*[;,]\s*").First().ToLower();
            }
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
            get
            {
                if (!Environment.ContainsKey("rack.input") || Environment["rack.input"] == null)
                {
                    throw new InvalidOperationException("Missing rack.input");
                }

                if (Environment.ContainsKey("rack.request.form_input") &&
                    Environment["rack.request.form_input"] == Environment["rack.input"])
                {
                    return Environment["rack.request.form_hash"];
                }

                if (HasFormData || HasParseableData)
                {
                    Environment["rack.request.form_input"] = Environment["rack.input"];

                    if (Environment.ContainsKey("rack.request.form_hash") && Environment["rack.request.form_hash"] == ParseMultipart(Environment))
                    {

                        return Environment["rack.request.form_hash"];
                    }

                    var input = ((MemoryStream)Environment["rack.input"]);
                    var inputBytes = new byte[input.Length];

                    input.Position = 0;
                    input.Read(inputBytes, 0, (int)input.Length);
                    var formVars = Encoding.ASCII.GetString(inputBytes);
                    if (formVars[formVars.Length - 1] == '0')
                    {
                        formVars = formVars.Substring(0, formVars.Length - 1);
                    }

                    Environment["rack.request.form_vars"] = formVars;
                    Environment["rack.request.form_hash"] = ParseQuery(formVars);

                    input.Position = 0;

                    return Environment["rack.request.form_hash"];
                }
                return new Dictionary<string, string>();
            }
        }

        protected dynamic ParseMultipart(IDictionary<string, dynamic> environment)
        {
            // TODO: Handle multipart form data.
            return null;
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