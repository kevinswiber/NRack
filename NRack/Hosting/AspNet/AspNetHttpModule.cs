using System;
using System.Configuration;
using System.Web;

namespace NRack.Hosting.AspNet
{
    public class AspNetHttpModule : IHttpModule
    {
        public static Builder Builder { get; set; }

        #region Implementation of IHttpModule

        public void Init(HttpApplication context)
        {
            Builder = new Builder();

            var rackConfigSection = (RackConfigurationSection)ConfigurationManager.GetSection("rack");
            
            var rackConfig = ((RackConfigBase)Activator.CreateInstance(Type.GetType(rackConfigSection.Type)));
            rackConfig.Builder = Builder;
            rackConfig.RackUp();
        }

        public void Dispose()
        {
            
        }

        #endregion
    }
}