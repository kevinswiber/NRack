using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRack.Adapters;

namespace NRack
{
    public class ContentLength : IApplication
    {
        public int[] StatusWithNoEntityBody =
            Enumerable.Range(100, 99).ToArray().Concat(new[]  {204, 304}).ToArray();

        private readonly dynamic _app;

        public ContentLength(dynamic app)
        {
            _app = app;
        }

        #region Implementation of IApplication

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var response = _app.Call(environment);
            var status = response[0];
            var headers = response[1];
            var body = response[2];

            headers = new HeaderHash(headers);

            SetContentLength(status, headers, body);

            return new[] {status, headers, body};
        }

        #endregion

        private void SetContentLength(dynamic status, dynamic headers, dynamic body)
        {
            if (StatusWithNoEntityBody.Contains((int) status) || headers.ContainsKey("Content-Length") ||
                headers.ContainsKey("Transfer-Encoding"))
            {
                return;
            }

            var setHeader = CanSetHeader(body);

            if (!setHeader)
            {
                return;
            }

            var length = 0;

            body.Each((Action<object>)(part => length += Encoding.ASCII.GetByteCount((string)part)));

            headers["Content-Length"] = length.ToString();
        }

        private static bool CanSetHeader(dynamic body)
        {
            var setHeader = true;

            if (!(body is IEnumerable))
            {
                if (body is IIterable)
                {
                    if (!(body.Subject is IEnumerable))
                    {
                        setHeader = false;
                    }
                }
                else
                {
                    setHeader = false;
                }
            }
            return setHeader;
        }
    }
}