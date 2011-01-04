using System;
using System.Collections.Generic;

namespace NRack.Configuration
{
    public abstract class ConfigBase
    {
        private Builder _builder;

        public abstract void Start();

        public void ExecuteStart(Builder builder)
        {
            _builder = builder;
            Start();
        }

        public Builder Use<T>(params dynamic[] parameters)
        {
            return _builder.Use<T>(parameters);
        }

        public Builder Use(Type rackApplicationType, params dynamic[] parameters)
        {
            return _builder.Use(rackApplicationType, parameters);
        }

        public Builder Use(Func<IDictionary<string, dynamic>, dynamic[]> application)
        {
            return _builder.Use(application);
        }

        public Builder Run(dynamic rackApplicationType)
        {
            return _builder.Run(rackApplicationType);
        }

        public Builder Run(Func<IDictionary<string, dynamic>, dynamic[]> application)
        {
            return _builder.Run(application);
        }

        public Builder Map(string url, Action<Builder> action)
        {
           return _builder.Map(url, action);
        }

        public Builder Map(string url, Func<IDictionary<string, dynamic>, dynamic[]> application)
        {
            return _builder.Map(url, application);
        }
    }
}