using System;
using System.Collections.Generic;
using System.Linq;

namespace Rack.Example
{
    public class EnvironmentOutput
    {
        private readonly dynamic _application;
        private dynamic _response;
        private IDictionary<string, object> _environment;

        public EnvironmentOutput(dynamic application)
        {
            _application = application;
        }

        public dynamic[] Call(IDictionary<string, object> environment)
        {
            _environment = environment;

            if (_application != null)
            {
                var response = _application.Call(environment);
                _response = response[2];
                return new[] { response[0], response[1], this };
            }

            return new dynamic[] {200, new Headers{{"Content-Type", "text/html"}}, this};
        }

        public void Each(Action<object> action)
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