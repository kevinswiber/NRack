using System;
using System.Collections.Generic;
using NRack.Adapters;

namespace NRack.Example.AspNet
{
    public class PlainTextFilter : IApplication, IIterable
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

            return new[] { response[0], new Hash {{"Content-Type", "text/plain"}}, this };
        }

        public void Each(Action<dynamic> action)
        {
            _response.Each(action);
        }
    }
}