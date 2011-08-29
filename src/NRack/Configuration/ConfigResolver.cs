using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;

namespace NRack.Configuration
{
    public class ConfigResolver
    {
        public static ConfigBase GetRackConfigInstance()
        {
            var rackConfigSection = (RackConfigurationSection)ConfigurationManager.GetSection("rack");

            var rackConfigType = (rackConfigSection != null && !string.IsNullOrEmpty(rackConfigSection.Type))
                                     ? Type.GetType(rackConfigSection.Type)
                                     : GetRackConfigTypeFromReferencedAssemblies();

            return ((ConfigBase)Activator.CreateInstance(rackConfigType));
        }

        private static Type GetRackConfigTypeFromReferencedAssemblies()
        {

            var referencedAssemblies = IsRunningInAspNet()
                                           ? BuildManager.GetReferencedAssemblies()
                                           : AppDomain.CurrentDomain.GetAssemblies();

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

        private static bool IsRunningInAspNet()
        {
            return HttpRuntime.AppDomainAppId != null;
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