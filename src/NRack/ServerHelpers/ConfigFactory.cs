using System;
using NRack.Configuration;

namespace NRack.ServerHelpers
{
    public class ConfigFactory
    {
        public static ConfigBase NewInstance()
        {
            var configType = ConfigFinder.FindType();
            return ((ConfigBase) Activator.CreateInstance(configType));
        }

        public static ConfigBase NewInstance(Type type)
        {
            return ((ConfigBase) Activator.CreateInstance(type));
        }
    }
}