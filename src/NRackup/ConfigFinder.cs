using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NRack.Configuration;

namespace NRackup
{
    public class ConfigFinder
    {
        public Type FindType()
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