using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Rack.Hosting.AspNet
{
    public class AspNetHandler : IHttpHandler
    {
        #region Implementation of IHttpHandler

        public void ProcessRequest(HttpContext context)
        {
            var rawEnvironment = context.Request.Params;
            Dictionary<string, object> environment =
                rawEnvironment.AllKeys.ToDictionary(key => key, key => (object)rawEnvironment[key]);

            if ((string)environment["SCRIPT_NAME"] == string.Empty)
            {
                environment["SCRIPT_NAME"] = "/";
            }

            var rackEnvs = new Dictionary<string, object>
                               {
                                   {"rack.version", RackVersion.Version},
                                   {"rack.input", context.Request.InputStream},
                                   {"rack.errors", Console.OpenStandardError()},
                                   {"rack.multithread", true},
                                   {"rack.multiprocess", false},
                                   {"rack.run_once", "false"},
                                   {"rack.url_scheme", "http"}
                               };

            environment = environment.Union(rackEnvs).ToDictionary(key => key.Key, val => val.Value);

            if (!environment.ContainsKey("SCRIPT_NAME"))
            {
                environment["SCRIPT_NAME"] = string.Empty;
            }

            var responseArray = AspNetHttpModule.Builder.Call(environment);

            var response = AspNetResponse.Create(responseArray);

            context.Response.StatusCode = response.StatusCode;

            //context.Response.Headers.Add(response.Headers);
            if (response.Headers != null)
            {
                foreach (var key in response.Headers.AllKeys)
                {
                    context.Response.AddHeader(key, response.Headers[key]);
                }
            }

            if (response.Body is string)
            {
                context.Response.Write(response.Body);
            }
            else
            {
                response.Body.Each((Action<object>)(body => context.Response.Write(body)));
            }

            var methodInfos = (IEnumerable<MethodInfo>)response.Body.GetType().GetMethods();
            
            var closeMethods = Enumerable.Where(methodInfos, method => method.Name == "Close");

            foreach(var method in closeMethods)
            {
                if(method.GetParameters().Length == 0 && method.ReturnType == typeof(void))
                {
                    method.Invoke(response.Body, null);
                    break;
                }
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        #endregion
    }
}