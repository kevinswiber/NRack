using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using NRack.Adapters;

namespace NRack.Mock
{
    public class MockRequest
    {
        class FatalWarningException : InvalidOperationException
        {
            public FatalWarningException(string message)
                : base(message)
            {
            }
        }

        class FatalWarner
        {
            public void Puts(string warning)
            {
                throw new FatalWarningException(warning);
            }

            public void Write(string warning)
            {
                throw new FatalWarningException(warning);
            }

            public void Flush()
            {
            }

            public override string ToString()
            {
                return string.Empty;
            }
        }

        private Dictionary<string, dynamic> DefaultEnv = new Dictionary<string, dynamic>
            {
                {"rack.version", RackVersion.Version},
                {"rack.input", new MemoryStream()},
                {"rack.errors", new MemoryStream()},
                {"rack.multithread", true},
                {"rack.multiprocess", true},
                {"rack.run_once", false}
            };

        public MockRequest(dynamic app)
        {
            App = app;
        }

        public dynamic App { get; private set; }

        public dynamic Get(string uri, Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            return Request("GET", uri, opts);
        }

        public dynamic Post(string uri, Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            return Request("POST", uri, opts);
        }

        public dynamic Put(string uri, Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            return Request("PUT", uri, opts);
        }

        public dynamic Delete(string uri, Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            return Request("Delete", uri, opts);
        }

        public dynamic Request(string method = "GET", string uri = "", Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            opts["method"] = method;

            var env = EnvironmentFor(uri, opts);

            dynamic app = opts.ContainsKey("lint") ? null /*new Lint(App)*/ : App;

            var errors = env["rack.errors"];
            var response = app.Call(env);
            var returnResponse = new dynamic[response.Length + 1];
            Array.Copy(response, returnResponse, response.Length);

            returnResponse[response.Length] = errors;

            return new MockResponse(returnResponse);
        }

        public Dictionary<string, dynamic> EnvironmentFor(string uri = "", Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            var env = new Dictionary<string, dynamic>();
            env["rack.version"] = RackVersion.Version;
            env["rack.input"] = new MemoryStream();
            env["rack.errors"] = new MemoryStream();
            env["rack.multithread"] = true;
            env["rack.multiprocess"] = true;
            env["rack.run_once"] = false;

            if (!uri.StartsWith("/"))
            {
                uri = "/" + uri;
            }

            var newUri = new Uri(uri, UriKind.RelativeOrAbsolute);

            env["REQUEST_METHOD"] = opts.ContainsKey("method") ? opts["method"].ToUpper() : "GET";

            if (newUri.IsAbsoluteUri)
            {
                env["SERVER_NAME"] = !string.IsNullOrEmpty(newUri.Host) ? newUri.Host : "example.org";
                env["SERVER_PORT"] = !string.IsNullOrEmpty(newUri.Port.ToString()) ? newUri.Port.ToString() : "80";
                env["QUERY_STRING"] = !string.IsNullOrEmpty(newUri.Query) ? newUri.Query : string.Empty;
                
                var virtualPath = newUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
                env["PATH_INFO"] = string.IsNullOrEmpty(virtualPath) ? "/" : virtualPath;
                
                env["rack.url_scheme"] = string.IsNullOrEmpty(newUri.Scheme) ? "http" : newUri.Scheme;
                
                env["HTTPS"] = env["rack.url_scheme"] == "https" ? "on" : "off";
            }
            else
            {
                env["SERVER_NAME"] = "example.org";
                env["SERVER_PORT"] = 80;
                env["QUERY_STRING"] = string.Empty;
                env["PATH_INFO"] = string.IsNullOrEmpty(newUri.OriginalString) ? "/" : newUri.OriginalString;
                env["rack.url_scheme"] = "http";
                env["HTTPS"] = "off";
            }
            
            env["SCRIPT_NAME"] = opts.ContainsKey("script_name") ? opts["script_name"] : string.Empty;

            if (opts.ContainsKey("fatal"))
            {
                env["rack.errors"] = new FatalWarner();
            }
            else
            {
                env["rack.errors"] = new MemoryStream();
            }

            if (opts.ContainsKey("params"))
            {
                var parameters = opts["params"];

                if (env["REQUEST_METHOD"] == "GET")
                {
                    if (parameters is string)
                    {
                        parameters = new Utils().ParseNestedQuery(parameters);
                    }
                    foreach(var parm in new Utils().ParseNestedQuery(env["QUERY_STRING"]))
                    {
                        parameters[parm.Key] = parm.Value;
                    }
                }

                // TODO: Finish implementing.
            }

            if (opts.ContainsKey("input") && opts["input"] == null)
            {
                opts["input"] = string.Empty;
            }

            if (opts.ContainsKey("input"))
            {
                dynamic rackInput = null;

                if (opts["input"] is string)
                {
                    var encoder = new UTF8Encoding();
                    rackInput = new MemoryStream(encoder.GetBytes(opts["input"]));
                }
                else
                {
                    rackInput = opts["input"];
                }

                env["rack.input"] = rackInput;

                env["CONTENT_LENGTH"] = env["rack.input"].Length.ToString();
            }

            foreach(var item in opts)
            {
                env[item.Key] = item.Value;
            }

            return env;
        }
    }

    public class MockResponse
    {
        public MockResponse(dynamic[] responseArray)
        {
            Status = Convert.ToInt32(responseArray[0]);
            OriginalHeaders = responseArray[1];
            Headers = (Headers) responseArray[1];

            var bodyList = new List<string>();
            var body = responseArray[2] as IResponseBody;

            if (body == null)
            {
                body = new EnumerableBodyAdapter(responseArray[2]);
            }


            body.Each(part => bodyList.Add(part.ToString()));

            Body = new EnumerableBodyAdapter(bodyList);

            var errors = responseArray.Length > 3 ? responseArray[3] : new MemoryStream();
            if (errors is MemoryStream)
            {
                var errorStream = (MemoryStream) errors;

                var pos = errorStream.Position;
                errorStream.Position = 0;

                var reader = new StreamReader(errorStream);
                var errorString = reader.ReadToEnd();

                errorStream.Position = pos;

                Errors = errorString;
            }
        }

        public string this[string name]
        {
            get { return Headers[name]; } 
        }

        public int Status { get; private set; }
        public Headers OriginalHeaders { get; private set; }
        public Headers Headers { get; private set; }
        public IResponseBody Body { get; private set; }
        public string Errors { get; set; }
    }
}