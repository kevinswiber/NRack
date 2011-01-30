using System.Collections.Generic;
using System.Linq;
using NRack.Helpers;
using NRack.Mock;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class UrlMap_Specs
    {
        [Test]
        public void Should_Dispatch_Paths_Correctly()
        {
            var app = DetachedApplication.Create
                (env =>
                    new dynamic[] {200, 
                        new Hash
                            {
                                {"X-ScriptName", env["SCRIPT_NAME"]}, 
                                {"X-PathInfo", env["PATH_INFO"]},
                                {"Content-Type", "text/plain"}
                            },
                        string.Empty});

            var map = new UrlMap(new Dictionary<string, object>
                                     {
                                         {"http://foo.org/bar", app},
                                         {"/foo", app},
                                         {"/foo/bar", app}
                                     });

            var res = new MockRequest(map).Get("/");
            Assert.AreEqual(404, res.Status);

            res = new MockRequest(map).Get("/qux");
            Assert.AreEqual(404, res.Status);

            res = new MockRequest(map).Get("/foo");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo", res["X-ScriptName"]);
            Assert.AreEqual(string.Empty, res["X-PathInfo"]);

            res = new MockRequest(map).Get("/foo/");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo", res["X-ScriptName"]);
            Assert.AreEqual("/", res["X-PathInfo"]);

            res = new MockRequest(map).Get("/foo/bar");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo/bar", res["X-ScriptName"]);
            Assert.AreEqual(string.Empty, res["X-PathInfo"]);

            res = new MockRequest(map).Get("/foo/bar/");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo/bar", res["X-ScriptName"]);
            Assert.AreEqual("/", res["X-PathInfo"]);

            res = new MockRequest(map).Get("/foo///bar//quux");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo/bar", res["X-ScriptName"]);
            Assert.AreEqual("//quux", res["X-PathInfo"]);

            res = new MockRequest(map).Get("/foo/quux", new Dictionary<string, object> { { "SCRIPT_NAME", "/bleh" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/bleh/foo", res["X-ScriptName"]);
            Assert.AreEqual("/quux", res["X-PathInfo"]);

            res = new MockRequest(map).Get("/bar", new Dictionary<string, object> { { "HTTP_HOST", "foo.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/bar", res["X-ScriptName"]);
            Assert.AreEqual(string.Empty, res["X-PathInfo"]);

            res = new MockRequest(map).Get("/bar/", new Dictionary<string, object> { { "HTTP_HOST", "foo.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/bar", res["X-ScriptName"]);
            Assert.AreEqual("/", res["X-PathInfo"]);
        }

        [Test]
        public void Should_Dispatch_Hosts_Correctly()
        {
            var mapping = new KeyValuePair<string, object>[4];
            mapping[0] = GetMapForDispatchTest("http://foo.org/", "foo.org");
            mapping[1] = GetMapForDispatchTest("http://subdomain.foo.org/", "subdomain.foo.org");
            mapping[2] = GetMapForDispatchTest("http://bar.org/", "bar.org");
            mapping[3] = GetMapForDispatchTest("/", "default.org");

            var mappingDictionary = mapping.ToDictionary(pair => pair.Key, pair => pair.Value);

            var map = new UrlMap(mappingDictionary);

            var res = new MockRequest(map).Get("/");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("default.org", res["X-Position"]);

            res = new MockRequest(map).Get("/", new Dictionary<string, object> { { "HTTP_HOST", "bar.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("bar.org", res["X-Position"]);

            res = new MockRequest(map).Get("/", new Dictionary<string, object> { { "HTTP_HOST", "foo.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("foo.org", res["X-Position"]);

            res = new MockRequest(map).Get("/",
                                           new Dictionary<string, object> { { "HTTP_HOST", "subdomain.foo.org" }, { "SERVER_NAME", "foo.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("subdomain.foo.org", res["X-Position"]);

            res = new MockRequest(map).Get("http://foo.org/");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("foo.org", res["X-Position"]);

            res = new MockRequest(map).Get("/", new Dictionary<string, object> { { "HTTP_HOST", "example.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("default.org", res["X-Position"]);

            res = new MockRequest(map).Get("/",
                                           new Dictionary<string, object> { { "HTTP_HOST", "example.org:9292" }, { "SERVER_PORT", "9292" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("default.org", res["X-Position"]);
        }

        [Test]
        public void Should_Be_Nestable()
        {
            var map = new UrlMap(new Dictionary<string, object>{
            {"/foo", 
                new UrlMap(new Dictionary<string, object>{
                    {"/bar", 
                        new UrlMap(new Dictionary<string, object>{
                            {"/quux",
                    DetachedApplication.Create(env =>
                        new dynamic[] {200, new Hash
                                                {
                                                    {"Content-Type", "text/plain"},
                                                    {"X-Position", "/foo/bar/quux"},
                                                    {"X-PathInfo", env["PATH_INFO"]},
                                                    {"X-ScriptName", env["SCRIPT_NAME"]}
                                                }, string.Empty})}})}})}});

            var res = new MockRequest(map).Get("/foo/bar");
            Assert.AreEqual(404, res.Status);

            res = new MockRequest(map).Get("/foo/bar/quux");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo/bar/quux", res["X-Position"]);
            Assert.AreEqual(string.Empty, res["X-PathInfo"]);
            Assert.AreEqual("/foo/bar/quux", res["X-ScriptName"]);
        }

        [Test]
        public void Should_Route_Root_Apps_Correctly()
        {
            var mapping = new KeyValuePair<string, object>[2];
            mapping[0] = GetMapForRootRouteTest("/", "root");
            mapping[1] = GetMapForRootRouteTest("/foo", "foo");

            var mappingDictionary = mapping.ToDictionary(pair => pair.Key, pair => pair.Value);

            var map = new UrlMap(mappingDictionary);

            var mock = new MockRequest(map);

            var res = mock.Get("/foo/bar");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("foo", res["X-Position"]);
            Assert.AreEqual("/bar", res["X-PathInfo"]);
            Assert.AreEqual("/foo", res["X-ScriptName"]);

            res = mock.Get("/foo");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("foo", res["X-Position"]);
            Assert.AreEqual(string.Empty, res["X-PathInfo"]);
            Assert.AreEqual("/foo", res["X-ScriptName"]);

            res = mock.Get("/bar");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("root", res["X-Position"]);
            Assert.AreEqual("/bar", res["X-PathInfo"]);
            Assert.AreEqual(string.Empty, res["X-ScriptName"]);

            res = mock.Get(string.Empty);
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("root", res["X-Position"]);
            Assert.AreEqual("/", res["X-PathInfo"]);
            Assert.AreEqual(string.Empty, res["X-ScriptName"]);
        }

        [Test]
        public void Should_Not_Squeeze_Slashes()
        {
            var mapping = new KeyValuePair<string, object>[2];
            mapping[0] = GetMapForRootRouteTest("/", "root");
            mapping[1] = GetMapForRootRouteTest("/foo", "foo");

            var mappingDictionary = mapping.ToDictionary(pair => pair.Key, pair => pair.Value);

            var map = new UrlMap(mappingDictionary);

            var mock = new MockRequest(map);

            var res = mock.Get("/http://example.org/bar");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("root", res["X-Position"]);
            Assert.AreEqual("/http://example.org/bar", res["X-PathInfo"]);
            Assert.AreEqual(string.Empty, res["X-ScriptName"]);

        }

        private static KeyValuePair<string, object> GetMapForDispatchTest(string uri, string xPosition)
        {
            return new KeyValuePair<string, object>(uri,
                    DetachedApplication.Create(env =>
                        new dynamic[] {200, new Hash
                                                {
                                                    {"Content-Type", "text/plain"},
                                                    {"X-Position", xPosition},
                                                    {"X-Host", env.ContainsKey("HTTP_HOST") 
                                                        ? env["HTTP_HOST"] 
                                                        : env["SERVER_NAME"]}
                                                }, string.Empty}));
        }

        private static KeyValuePair<string, object> GetMapForRootRouteTest(string uri, string xPosition)
        {
            return new KeyValuePair<string, object>(uri,
                    DetachedApplication.Create(env =>
                        new dynamic[] {200, new Hash
                                                {
                                                    {"Content-Type", "text/plain"},
                                                    {"X-Position", xPosition},
                                                    {"X-PathInfo", env["PATH_INFO"]},
                                                    {"X-ScriptName", env["SCRIPT_NAME"]}
                                                }, string.Empty}));
        }
    }
}