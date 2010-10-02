using System;

namespace Rack.Hosting.AspNet
{
    public abstract class RackConfigBase
    {
        public Builder Builder { get; set; }

        protected RackConfigBase()
        {
            Builder = new Builder();
        }

        public abstract void RackUp();

        public void Use<T>(params dynamic[] parameters)
        {
            Use(typeof(T), parameters);
        }

        public void Use(Type rackApplicationType, params dynamic[] parameters)
        {
            Builder.Use(rackApplicationType, parameters);
        }

        public void Run(dynamic rackApplicationType)
        {
            Builder.Run(rackApplicationType);
        }

        public void Map(string url, Action<Builder> action)
        {
            Builder.Map(url, action);
        }
    }
}