using System;
using System.Collections.Generic;
using System.IO;

namespace NRack.Mock
{
    public class MockRequest
    {
        class FatalWarningException : InvalidOperationException
        {
            public FatalWarningException(string message) : base(message)
            {
            }
        }

        class FatalWarner
        {
            public void Puts(string warning)
            {
                throw new FatalWarningException(warning);
            }

            public void Write(string warning)
            {
                throw new FatalWarningException(warning);
            }

            public void Flush()
            {
            }

            public override string ToString()
            {
                return string.Empty;
            }
        }

        private Dictionary<string, dynamic> DefaultEnv = new Dictionary<string, dynamic>
            {
                {"rack.version", RackVersion.Version},
                {"rack.input", new MemoryStream()},
                {"rack.errors", new MemoryStream()},
                {"rack.multithread", true},
                {"rack.multiprocess", true},
                {"rack.run_once", false}
            };

        public MockRequest(dynamic app)
        {
            App = app;
        }

        public dynamic App { get; private set; }

        public dynamic Get(string uri, Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            return Request("GET", uri, opts);
        }

        public dynamic Post(string uri, Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            return Request("POST", uri, opts);
        }

        public dynamic Put(string uri, Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            return Request("PUT", uri, opts);
        }

        public dynamic Delete(string uri, Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            return Request("Delete", uri, opts);
        }

        public dynamic Request(string method = "GET", string uri = "", Dictionary<string, dynamic> opts = null)
        {
            if (opts == null)
            {
                opts = new Dictionary<string, dynamic>();
            }

            opts["method"] = method;

            var env = EnvironmentFor(uri, opts);

            dynamic app = opts.ContainsKey("lint") ? null /*new Lint(App)*/ : App;

            var errors = env["rack.errors"];
            return new MockResponse((app.Call(env) + errors));
        }

        public Dictionary<string, dynamic> EnvironmentFor(string uri = "", Dictionary<string, dynamic> opts = null)
        {
            return new Dictionary<string, dynamic>();
        }
    }

    public class MockResponse
    {
        public MockResponse(dynamic o)
        {
            throw new NotImplementedException();
        }
    }
}