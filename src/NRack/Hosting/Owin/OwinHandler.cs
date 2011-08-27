using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NRack.Configuration;

namespace NRack.Hosting.Owin
{
    // cancel

    using ResponseCallBack = Action<string, // Response Status
                                IDictionary<string, string>, // Response Headers
                                Func<Func<ArraySegment<byte>, Action, bool>, Action<Exception>, Action, Action>>; // BodyDelegate

    public class OwinHandler
    {
        public void ProcessRequest(IDictionary<string, object> environment, 
            ResponseCallBack responseCallBack, Action<Exception> errorCallback)
        {
            var nrackEnvironment = new Dictionary<string, dynamic>();
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
                var host = nrackEnvironment["HTTP_HOST"];
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

            var config = GetRackConfigInstance();
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

        public ConfigBase GetRackConfigInstance()
        {
            var rackConfigSection = (RackConfigurationSection)ConfigurationManager.GetSection("rack");

            var rackConfigType = (rackConfigSection != null && !string.IsNullOrEmpty(rackConfigSection.Type))
                                     ? Type.GetType(rackConfigSection.Type)
                                     : GetRackConfigTypeFromReferencedAssemblies();

            return ((ConfigBase)Activator.CreateInstance(rackConfigType));
        }

        private static Type GetRackConfigTypeFromReferencedAssemblies()
        {
            var referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
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
    }
}