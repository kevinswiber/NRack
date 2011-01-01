using System;
using System.Collections.Generic;
using System.Linq;
using NRack.Adapters;

namespace NRack.Example.AspNet
{
    public class EnvironmentOutput : IApplication, IIterable
    {
        private readonly dynamic _application;
        private dynamic _response;
        private IDictionary<string, dynamic> _environment;

        public EnvironmentOutput(dynamic application)
        {
            _application = application;
        }

        public EnvironmentOutput() : this(null)
        {
        }

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            _environment = environment;

            if (_application != null)
            {
                var response = _application.Call(environment);
                _response = response[2];
                return new[] { response[0], response[1], this };
            }

            return new dynamic[] {200, new Hash{{"Content-Type", "text/html"}}, this};
        }

        public void Each(Action<dynamic> action)
        {
            if (_response != null)
            {
                _response.Each(action);
            }

            var envOutput = _environment.Keys
                .Aggregate("", (current, key) => current + string.Format("<li>{0}={1}</li>", key, _environment[key].ToString()));

            action(envOutput);
        }
    }
}