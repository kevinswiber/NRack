using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using NRack.Configuration;

namespace NRack.Hosting.AspNet
{
    public class AspNetHandler : IHttpAsyncHandler
    {
        private Action<HttpContext> _requestProcessor;
        private Func<Builder> _getBuilderInContext;
        private ConfigBase _config;

        #region Implementation of IHttpHandler

        public void ProcessRequest(HttpContext context)
        {
            if (_config == null)
            {
                _config = GetRackConfigInstance();
                _getBuilderInContext = () => new Builder(_config.ExecuteStart);
            }

            var rawEnvironment = context.Request.ServerVariables;
            Dictionary<string, dynamic> environment =
                rawEnvironment.AllKeys.ToDictionary(key => key, key => (object)rawEnvironment[key]);

            environment["SCRIPT_NAME"] = string.Empty;

            //if ((string)environment["SCRIPT_NAME"] == string.Empty)
            //{
            //    environment["SCRIPT_NAME"] = "/";
            //}

            var rackEnvs = new Dictionary<string, dynamic>
                               {
                                   {"rack.version", RackVersion.Version},
                                   {"rack.input", context.Request.InputStream},
                                   {"rack.errors", Console.OpenStandardError()},
                                   {"rack.multithread", true},
                                   {"rack.multiprocess", false},
                                   {"rack.run_once", false},
                                   {"rack.url_scheme", context.Request.IsSecureConnection ? "https" : "http"}

                               };

            environment = environment.Union(rackEnvs).ToDictionary(key => key.Key, val => val.Value);

            //if (!environment.ContainsKey("SCRIPT_NAME"))
            //{
            //    environment["SCRIPT_NAME"] = string.Empty;
            //}

            var builder = _getBuilderInContext();
            var responseArray = builder.Call(environment);

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

            response.Body.Each((Action<dynamic>)(body =>
                {
                    if (body is string)
                    {
                        context.Response.Write(body);
                    }
                    else if (body is byte[])
                    {
                        context.Response.BinaryWrite(body);
                    }
                }));

            var methodInfos = (IEnumerable<MethodInfo>)response.Body.GetType().GetMethods();

            var closeMethods = Enumerable.Where(methodInfos, method => method.Name == "Close");

            foreach (var method in closeMethods)
            {
                if (method.GetParameters().Length == 0 && method.ReturnType.Equals(typeof(void)))
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

        private ConfigBase GetRackConfigInstance()
        {
            var rackConfigSection = (RackConfigurationSection)ConfigurationManager.GetSection("rack");

            var rackConfigType = (rackConfigSection != null && !string.IsNullOrEmpty(rackConfigSection.Type))
                                     ? Type.GetType(rackConfigSection.Type)
                                     : GetRackConfigTypeFromReferencedAssemblies();

            return ((ConfigBase)Activator.CreateInstance(rackConfigType));
        }

        private static Type GetRackConfigTypeFromReferencedAssemblies()
        {
            var referencedAssemblies = BuildManager.GetReferencedAssemblies();
            IEnumerable<Type> typesCollected = Type.EmptyTypes;
            foreach (Assembly assembly in referencedAssemblies)
            {
                Type[] typesInAsm;
                try
                {
                    typesInAsm = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    typesInAsm = ex.Types;
                }

                typesCollected = typesCollected.Concat(typesInAsm);
            }

            var rackConfigTypes = typesCollected.Where(type => TypeIsPublicClass(type) && TypeIsRackConfig(type));

            Type rackConfig = rackConfigTypes.First();

            return rackConfig;
        }

        private static bool TypeIsPublicClass(Type type)
        {
            return (!type.Equals(null) && type.IsPublic && type.IsClass && !type.IsAbstract);
        }

        private static bool TypeIsRackConfig(Type type)
        {
            return typeof(ConfigBase).IsAssignableFrom(type);
        }

        #region Implementation of IHttpAsyncHandler

        /// <summary>
        /// Initiates an asynchronous call to the HTTP handler.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> that contains information about the status of the process.
        /// </returns>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests. </param><param name="cb">The <see cref="T:System.AsyncCallback"/> to call when the asynchronous method call is complete. If <paramref name="cb"/> is null, the delegate is not called. </param><param name="extraData">Any extra data needed to process the request. </param>
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            _requestProcessor = ProcessRequest;
            return _requestProcessor.BeginInvoke(context, cb, extraData);
        }

        /// <summary>
        /// Provides an asynchronous process End method when the process ends.
        /// </summary>
        /// <param name="result">An <see cref="T:System.IAsyncResult"/> that contains information about the status of the process. </param>
        public void EndProcessRequest(IAsyncResult result)
        {
            _requestProcessor.EndInvoke(result);
        }

        #endregion
    }
}