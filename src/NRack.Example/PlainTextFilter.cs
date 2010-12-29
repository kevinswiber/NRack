using System;
using System.Collections.Generic;
using NRack.Adapters;

namespace NRack.Example
{
    public class PlainTextFilter : IApplication, IResponseBody
    {
        private readonly dynamic _application;
        private dynamic _response;

        public PlainTextFilter(dynamic application)
        {
            _application = application;
        }

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var response = _application.Call(environment);
            _response = response[2];

            return new[] { response[0], new Headers {{"Content-Type", "text/plain"}}, this };
        }

        public void Each(Action<object> action)
        {
            _response.Each(action);
        }
    }
}