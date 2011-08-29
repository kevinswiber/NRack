using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NRack.Configuration;

namespace NRack.Hosting.Owin
{
    using ResponseCallBack = Action<string, // Response Status
                                IDictionary<string, string>, // Response Headers
                                Func<Func<ArraySegment<byte>, Action, bool>, Action<Exception>, Action, Action>>; // BodyDelegate

    public class OwinHandler
    {
        public void ProcessRequest(IDictionary<string, object> environment, 
            ResponseCallBack responseCallBack, Action<Exception> errorCallback)
        {
            Dictionary<string, dynamic> nrackEnvironment = GetNrackEnvironment(environment);

            var config = ConfigResolver.GetRackConfigInstance();
            var builder = new Builder(config.ExecuteStart);
            var response = new OwinResponseAdapter(builder.Call(nrackEnvironment));

            responseCallBack(response.Status, response.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()),
                (next, error, complete) =>
                    {
                        try
                        {
                            next(response.GetBody(), null);
                            complete();
                        }
                        catch (Exception ex)
                        {
                            error(ex);
                        }

                        return () => { };
                    });
        }

        private static Dictionary<string, dynamic> GetNrackEnvironment(IDictionary<string, object> environment)
        {
            Dictionary<string, dynamic> nrackEnvironment = environment.Keys.ToDictionary(key => key, key => environment[key]);

            nrackEnvironment["REQUEST_METHOD"] = environment["owin.RequestMethod"];

            nrackEnvironment["PATH_INFO"] = environment["owin.RequestPath"];
            nrackEnvironment["SCRIPT_NAME"] = environment["owin.RequestPathBase"];
            nrackEnvironment["QUERY_STRING"] = environment["owin.RequestQueryString"];

            var headers = environment["owin.RequestHeaders"] as Dictionary<string, string> ?? new Dictionary<string, string>();
            foreach(var key in headers.Keys)
            {
                var nrackKey = string.Concat("HTTP_", key.Replace("-", "_").ToUpper());
                nrackEnvironment[nrackKey] = headers[key];
            }

            if (nrackEnvironment.ContainsKey("HTTP_HOST"))
            {
                var host = nrackEnvironment["HTTP_HOST"].ToString();
                if (!string.IsNullOrEmpty(host))
                {
                    var splitHostString = ((string)nrackEnvironment["HTTP_HOST"]).Split(':');
                    if (splitHostString.Any())
                    {
                        nrackEnvironment["SERVER_NAME"] = splitHostString[0];

                        if (splitHostString.Length > 1)
                        {
                            nrackEnvironment["SERVER_PORT"] = splitHostString[1];
                        }
                    }
                }
            }

            var rackEnvs = new Dictionary<string, dynamic>
                               {
                                   {"rack.version", RackVersion.Version},
                                   {"rack.input", environment["owin.RequestBody"]},
                                   {"rack.errors", Console.OpenStandardError()},
                                   {"rack.multithread", true},
                                   {"rack.multiprocess", false},
                                   {"rack.run_once", false},
                                   {"rack.url_scheme", environment["owin.RequestScheme"]}
                               };

            nrackEnvironment = nrackEnvironment.Union(rackEnvs).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            
            return nrackEnvironment;
        }
    }
}