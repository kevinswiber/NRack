using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NRack.Adapters;
using NRack.Configuration;

namespace NRack.Example.Server
{
    public static class ServerBootstrap
    {
        public static void Start()
        {
            var options = new Hash { { ServerOptions.App, GetBuilderFunc() } };

            NRack.Server.Start(options);
        }

        static Proc GetBuilderFunc()
        {
            var config = GetRackConfigInstance();
            Func<IDictionary<string, object>, object> builderInContextFunc =
                env => new Builder(config.ExecuteRackUp).Call(env);

            return new Proc(builderInContextFunc);
        }

        private static ConfigBase GetRackConfigInstance()
        {
            //var rackConfigSection = (RackConfigurationSection)ConfigurationManager.GetSection("rack");

            //var rackConfigType = (rackConfigSection != null && !string.IsNullOrEmpty(rackConfigSection.Type))
            //                         ? Type.GetType(rackConfigSection.Type)
            //                         : GetRackConfigTypeFromReferencedAssemblies();

            var configType = GetRackConfigTypeFromReferencedAssemblies();

            return ((ConfigBase) Activator.CreateInstance(configType));
        }

        private static Type GetRackConfigTypeFromReferencedAssemblies()
        {
            var assemblyFiles = 
                new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                .GetFiles("*.dll", SearchOption.AllDirectories)
                .Union(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                .GetFiles("*.exe", SearchOption.AllDirectories));
            
            IEnumerable<Type> typesCollected = Type.EmptyTypes;

            foreach (var assemblyFile in assemblyFiles)
            {
                Type[] typesInAsm;
                try
                {
                    var assembly = Assembly.LoadFrom(assemblyFile.FullName);
                    typesInAsm = assembly.GetTypes();
                }
                catch (Exception)
                {
                    continue;
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