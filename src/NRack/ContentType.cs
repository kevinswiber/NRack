using System;
using System.Collections.Generic;
using NRack.Helpers;

namespace NRack
{
    public class ContentType : ICallable
    {
        private readonly dynamic _app;
        private readonly string _contentType;

        public ContentType(dynamic app, string contentType = "text/html")
        {
            _app = app;
            _contentType = contentType;
        }

        #region Implementation of ICallable

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var response = _app.Call(environment);
            var headers = new HeaderHash(response[1]);

            if (!headers.ContainsKey("Content-Type") || headers["Content-Type"] == null)
            {
                headers["Content-Type"] = _contentType;
            }

            return new[] {response[0], headers, response[2]};
        }

        #endregion
    }
}