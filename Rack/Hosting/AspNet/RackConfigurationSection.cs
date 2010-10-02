using System;
using System.Configuration;

namespace Rack.Hosting.AspNet
{
    public class RackConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }
    }
}