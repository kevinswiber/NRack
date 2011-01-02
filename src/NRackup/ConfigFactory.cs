using System;
using NRack.Configuration;

namespace NRackup
{
    public class ConfigFactory
    {
        public static ConfigBase NewInstance()
        {
            var configType = new ConfigFinder().FindType();
            return ((ConfigBase) Activator.CreateInstance(configType));
        }
    }
}