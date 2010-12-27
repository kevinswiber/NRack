using System.Configuration;

namespace NRack.Hosting
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