using System;

namespace NRack.Configuration
{
    public abstract class RackConfigBase
    {
        private Builder _builder;

        public abstract void RackUp();

        public void ExecuteRackUp(Builder builder)
        {
            _builder = builder;
            RackUp();
        }

        public Builder Use<T>(params dynamic[] parameters)
        {
            return _builder.Use<T>(parameters);
        }

        public Builder Use(Type rackApplicationType, params dynamic[] parameters)
        {
            return _builder.Use(rackApplicationType, parameters);
        }

        public Builder Run(dynamic rackApplicationType)
        {
            return _builder.Run(rackApplicationType);
        }

        public Builder Map(string url, Action<Builder> action)
        {
           return _builder.Map(url, action);
        }
    }
}