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
    public class AspNetHandler : IHttpHandler
    {
        public static Func<Builder> GetBuilderInContext;
        public static ConfigBase Config;

        #region Implementation of IHttpHandler

        public void ProcessRequest(HttpContext context)
        {
            if (Config == null)
            {
                Config = GetRackConfigInstance();
                GetBuilderInContext = () => new Builder(Config.ExecuteRackUp);
            }

            var rawEnvironment = context.Request.Params;
            Dictionary<string, dynamic> environment =
                rawEnvironment.AllKeys.ToDictionary(key => key, key => (object)rawEnvironment[key]);

            if ((string)environment["SCRIPT_NAME"] == string.Empty)
            {
                environment["SCRIPT_NAME"] = "/";
            }

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

            if (!environment.ContainsKey("SCRIPT_NAME"))
            {
                environment["SCRIPT_NAME"] = string.Empty;
            }

            var builder = GetBuilderInContext();
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

            if (response.Body is string)
            {
                context.Response.Write(response.Body);
            }
            else
            {
                response.Body.Each((Action<dynamic>)(body => context.Response.Write(body)));
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
            return (type != null && type.IsPublic && type.IsClass && !type.IsAbstract);
        }

        private static bool TypeIsRackConfig(Type type)
        {
            return typeof(ConfigBase).IsAssignableFrom(type);
        }
    }
}