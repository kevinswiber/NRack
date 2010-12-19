using System;

namespace NRack
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

        public void Use<T>(params dynamic[] parameters)
        {
            _builder.Use<T>(parameters);
        }

        public void Use(Type rackApplicationType, params dynamic[] parameters)
        {
            _builder.Use(rackApplicationType, parameters);
        }

        public void Run(dynamic rackApplicationType)
        {
            _builder.Run(rackApplicationType);
        }

        public void Map(string url, Action<Builder> action)
        {
            _builder.Map(url, action);
        }
    }
}