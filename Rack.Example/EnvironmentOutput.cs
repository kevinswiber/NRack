using System;
using System.Collections.Generic;
using System.Linq;
using Rack;

namespace RackExample
{
    public class EnvironmentOutput
    {
        private readonly dynamic _application;
        private dynamic _response;
        private IDictionary<string, string> _environment;

        public EnvironmentOutput(dynamic application)
        {
            _application = application;
        }

        public dynamic[] Call(IDictionary<string, string> environment)
        {
            _environment = environment;

            var response = _application.Call(environment);
            _response = response[2];

            return new[] {response[0], response[1], this};
        }

        public void Each(Action<object> action)
        {
            _response.Each(action);

            var envOutput = _environment.Keys
                .Aggregate("", (current, key) => current + string.Format("<li>{0}={1}</li>", key, _environment[key]));

            action(envOutput);
        }
    }
}