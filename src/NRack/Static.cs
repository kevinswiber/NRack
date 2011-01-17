using System;
using System.Collections.Generic;
using System.Linq;
using NRack.Helpers;

namespace NRack
{
    public class Static : ICallable
    {
        private dynamic _app;
        private dynamic _urls;
        private File _fileServer;

        public Static(dynamic app, Hash options = null)
        {
            if (options == null)
            {
                options = new Hash();
            }

            _urls = options.ContainsKey("urls") ? options["urls"] : new[] { "/favicon.ico" };
            var root = options.ContainsKey("root") ? options["root"] : AppDomain.CurrentDomain.BaseDirectory;
            _fileServer = new File(root);
        }

        #region Implementation of ICallable

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            var path = environment["PATH_INFO"];

            bool canServe = false;

            if (_urls is Hash)
            {
                canServe = _urls.ContainsKey(path);
            }
            else if (_urls is IEnumerable<string>)
            {
                _urls = _urls as IEnumerable<string>;
                canServe = ((IEnumerable<string>)_urls).Any(url => ((string)path).IndexOf(url) == 0);
            }

            if (canServe)
            {
                if (_urls is Hash)
                {
                    environment["PATH_INFO"] = _urls[path];
                }
                return _fileServer.Call(environment);
            }
            else
            {
                return _app.Call(environment);
            }
        }

        #endregion
    }
}