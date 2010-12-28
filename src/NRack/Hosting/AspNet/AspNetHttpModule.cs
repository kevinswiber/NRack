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
    public class AspNetHttpModule : IHttpModule
    {
        #region Implementation of IHttpModule

        public void Init(HttpApplication context)
        {
            RackConfigBase rackConfig = GetRackConfigInstance();
            AspNetHandler.GetBuilderInContext = () => new Builder(rackConfig.ExecuteRackUp);
        }

        private RackConfigBase GetRackConfigInstance()
        {
            var rackConfigSection = (RackConfigurationSection)ConfigurationManager.GetSection("rack");

            var rackConfigType = (rackConfigSection != null && !string.IsNullOrEmpty(rackConfigSection.Type))
                                     ? Type.GetType(rackConfigSection.Type)
                                     : GetRackConfigTypeFromReferencedAssemblies();

            return ((RackConfigBase)Activator.CreateInstance(rackConfigType));
        }

        public void Dispose()
        {
            
        }

        #endregion

        private Type GetRackConfigTypeFromReferencedAssemblies()
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
            return typeof(RackConfigBase).IsAssignableFrom(type);
        }
    }
}