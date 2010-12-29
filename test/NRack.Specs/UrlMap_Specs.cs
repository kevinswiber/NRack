using System;
using System.Collections.Generic;
using System.Linq;
using NRack.Adapters;
using NRack.Mock;
using NUnit.Framework;

namespace NRack.Tests
{
    [TestFixture]
    public class UrlMap_Specs
    {
        [Test]
        public void Should_Dispatch_Paths_Correctly()
        {
            var app = new Proc(
                (Func<IDictionary<string, dynamic>, dynamic[]>)
                (env =>
                    new dynamic[] {200, 
                        new Headers
                            {
                                {"X-ScriptName", env["SCRIPT_NAME"].ToString()}, 
                                {"X-PathInfo", env["PATH_INFO"].ToString()},
                                {"Content-Type", "text/plain"}
                            },
                        string.Empty}));

            var map = new UrlMap(new Dictionary<string, dynamic>
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
            Assert.AreEqual("/foo", res["X-ScriptName"].ToString());
            Assert.AreEqual(string.Empty, res["X-PathInfo"].ToString());

            res = new MockRequest(map).Get("/foo/");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo", res["X-ScriptName"].ToString());
            Assert.AreEqual("/", res["X-PathInfo"].ToString());

            res = new MockRequest(map).Get("/foo/bar");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo/bar", res["X-ScriptName"].ToString());
            Assert.AreEqual(string.Empty, res["X-PathInfo"].ToString());

            res = new MockRequest(map).Get("/foo/bar/");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo/bar", res["X-ScriptName"].ToString());
            Assert.AreEqual("/", res["X-PathInfo"].ToString());

            res = new MockRequest(map).Get("/foo///bar//quux");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo/bar", res["X-ScriptName"].ToString());
            Assert.AreEqual("//quux", res["X-PathInfo"].ToString());

            res = new MockRequest(map).Get("/foo/quux", new Dictionary<string, dynamic> {{"SCRIPT_NAME", "/bleh"}});
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/bleh/foo", res["X-ScriptName"].ToString());
            Assert.AreEqual("/quux", res["X-PathInfo"].ToString());

            res = new MockRequest(map).Get("/bar", new Dictionary<string, dynamic> { { "HTTP_HOST", "foo.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/bar", res["X-ScriptName"].ToString());
            Assert.AreEqual(string.Empty, res["X-PathInfo"].ToString());

            res = new MockRequest(map).Get("/bar/", new Dictionary<string, dynamic> { { "HTTP_HOST", "foo.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/bar", res["X-ScriptName"].ToString());
            Assert.AreEqual("/", res["X-PathInfo"].ToString());
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

            res = new MockRequest(map).Get("/", new Dictionary<string, dynamic> { { "HTTP_HOST", "bar.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("bar.org", res["X-Position"]);

            res = new MockRequest(map).Get("/", new Dictionary<string, dynamic> { { "HTTP_HOST", "foo.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("foo.org", res["X-Position"]);

            res = new MockRequest(map).Get("/",
                                           new Dictionary<string, dynamic>
                                               {{"HTTP_HOST", "subdomain.foo.org"}, {"SERVER_NAME", "foo.org"}});
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("subdomain.foo.org", res["X-Position"]);

            res = new MockRequest(map).Get("http://foo.org/");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("default.org", res["X-Position"]);

            res = new MockRequest(map).Get("/", new Dictionary<string, dynamic> { { "HTTP_HOST", "example.org" } });
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("default.org", res["X-Position"]);

            res = new MockRequest(map).Get("/",
                                           new Dictionary<string, dynamic>
                                               {{"HTTP_HOST", "example.org:9292"}, {"SERVER_PORT", "9292"}});
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("default.org", res["X-Position"]);
        }

        [Test]
        public void Should_Be_Nestable()
        {
            var map = new UrlMap(new Dictionary<string, dynamic>{
            {"/foo", 
                new UrlMap(new Dictionary<string, dynamic>{
                    {"/bar", 
                        new UrlMap(new Dictionary<string, dynamic>{
                            {"/quux",
                    new Proc((Func<IDictionary<string, dynamic>, dynamic[]>)
                    (env =>
                        new dynamic[] {200, new Headers
                                                {
                                                    {"Content-Type", "text/plain"},
                                                    {"X-Position", "/foo/bar/quux"},
                                                    {"X-PathInfo", env["PATH_INFO"].ToString()},
                                                    {"X-ScriptName", env["SCRIPT_NAME"].ToString()}
                                                }, string.Empty}))}})}})}});

            var res = new MockRequest(map).Get("/foo/bar");
            Assert.AreEqual(404, res.Status);

            res = new MockRequest(map).Get("/foo/bar/quux");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("/foo/bar/quux", res["X-Position"].ToString());
            Assert.AreEqual(string.Empty, res["X-PathInfo"].ToString());
            Assert.AreEqual("/foo/bar/quux", res["X-ScriptName"].ToString());
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
            Assert.AreEqual("foo", res["X-Position"].ToString());
            Assert.AreEqual("/bar", res["X-PathInfo"].ToString());
            Assert.AreEqual("/foo", res["X-ScriptName"].ToString());

            res = mock.Get("/foo");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("foo", res["X-Position"].ToString());
            Assert.AreEqual(string.Empty, res["X-PathInfo"].ToString());
            Assert.AreEqual("/foo", res["X-ScriptName"].ToString());

            res = mock.Get("/bar");
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("root", res["X-Position"].ToString());
            Assert.AreEqual("/bar", res["X-PathInfo"].ToString());
            Assert.AreEqual(string.Empty, res["X-ScriptName"].ToString());

            res = mock.Get(string.Empty);
            Assert.AreEqual(200, res.Status);
            Assert.AreEqual("root", res["X-Position"].ToString());
            Assert.AreEqual("/", res["X-PathInfo"].ToString());
            Assert.AreEqual(string.Empty, res["X-ScriptName"].ToString());
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
            Assert.AreEqual("root", res["X-Position"].ToString());
            Assert.AreEqual("/http://example.org/bar", res["X-PathInfo"].ToString());
            Assert.AreEqual(string.Empty, res["X-ScriptName"].ToString());

        }

        private static KeyValuePair<string, object> GetMapForDispatchTest(string uri, string xPosition)
        {
            return new KeyValuePair<string, object>(uri,
                    new Proc((Func<IDictionary<string, dynamic>, dynamic[]>)
                    (env =>
                        new dynamic[] {200, new Headers
                                                {
                                                    {"Content-Type", "text/plain"},
                                                    {"X-Position", xPosition},
                                                    {"X-Host", env.ContainsKey("HTTP_HOST") 
                                                        ? env["HTTP_HOST"].ToString() 
                                                        : env["SERVER_NAME"].ToString()}
                                                }, string.Empty})));
        }

        private static KeyValuePair<string, object> GetMapForRootRouteTest(string uri, string xPosition)
        {
            return new KeyValuePair<string, object>(uri,
                    new Proc((Func<IDictionary<string, dynamic>, dynamic[]>)
                    (env =>
                        new dynamic[] {200, new Headers
                                                {
                                                    {"Content-Type", "text/plain"},
                                                    {"X-Position", xPosition},
                                                    {"X-PathInfo", env["PATH_INFO"].ToString()},
                                                    {"X-ScriptName", env["SCRIPT_NAME"].ToString()}
                                                }, string.Empty})));
        }
    }
}