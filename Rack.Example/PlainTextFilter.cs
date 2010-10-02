using System;
using System.Collections.Generic;
using Rack;

namespace RackExample
{
    public class PlainTextFilter
    {
        private readonly dynamic _application;
        private dynamic _response;

        public PlainTextFilter(dynamic application)
        {
            _application = application;
        }

        public dynamic[] Call(IDictionary<string, string> environment)
        {
            var response = _application.Call(environment);
            _response = response[2];

            return new dynamic[] { response[0], new Headers {{"Content-Type", "text/plain"}}, this };
        }

        public void Each(Action<dynamic> action)
        {
            _response.Each(action);
        }
    }
}