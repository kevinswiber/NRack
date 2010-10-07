using System.Collections.Generic;

namespace NRack.Example
{
    public class JavaScriptMinifier
    {
        private dynamic _app;
        private string _root;

        public JavaScriptMinifier(dynamic app, string root)
        {
            _app = app;
            _root = root;
        }

        public dynamic[] Call(IDictionary<string, object> env)
        {
            var root = _root;

            if (root.EndsWith("/"))
            {
                root = _root.Remove(_root.Length - 1);
            }

            var path = root + env["PATH_INFO"];
            
            if (!path.EndsWith(".js"))
            {
                return _app.Call(env);
            }

            return new dynamic[] {200, new Headers {{"Content-Type", "text/plain"}}, path};
        }
    }
}