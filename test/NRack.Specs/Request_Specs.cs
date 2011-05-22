using System;
using System.Collections.Generic;
using NRack.Helpers;
using NRack.Mock;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class Request_Specs
    {
        [Test]
        public void Should_Wrap_Rack_Variables()
        {
            var request = new Request(new MockRequest().EnvironmentFor("http://example.com:8080/"));

            Assert.AreEqual("http", request.Scheme);
            Assert.AreEqual("GET", request.RequestMethod);
            Assert.IsTrue(request.IsGet);
            Assert.IsFalse(request.IsPost);
            Assert.IsFalse(request.IsPut);
            Assert.IsFalse(request.IsDelete);
            Assert.IsFalse(request.IsHead);
            Assert.IsFalse(request.IsPatch);
            Assert.AreEqual(string.Empty, request.ScriptName);
            Assert.AreEqual("/", request.PathInfo);
            Assert.AreEqual(string.Empty, request.QueryString);
            Assert.AreEqual("example.com", request.Host);
            Assert.AreEqual(8080, request.Port);
            Assert.AreEqual(0, request.ContentLength);
            Assert.IsNull(request.ContentType);
        }

        [Test]
        public void Should_Figure_Out_The_Correct_Host()
        {
            var request = new Request(
                new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "www2.example.org" } }));

            Assert.AreEqual("www2.example.org", request.Host);

            request = new Request(
                new MockRequest().EnvironmentFor("/",
                new Hash { { "SERVER_NAME", "example.org" }, { "SERVER_PORT", "9292" } }));

            Assert.AreEqual("example.org", request.Host);

            request = new Request(
                new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org:9292" } }));

            Assert.AreEqual("example.org", request.Host);

            var env = new MockRequest().EnvironmentFor("/",
                new Hash { { "SERVER_ADDR", "192.168.1.1" }, { "SERVER_PORT", "9292" } });

            env.Remove("SERVER_NAME");
            request = new Request(env);
            Assert.AreEqual("192.168.1.1", request.Host);

            env = new MockRequest().EnvironmentFor("/");
            env.Remove("SERVER_NAME");
            request = new Request(env);

            Assert.AreEqual(string.Empty, request.Host);
        }

        [Test]
        public void Should_Figure_Out_The_Correct_Port()
        {
            var request =
                new Request(new MockRequest().EnvironmentFor("/", new Hash { { "HTTP_HOST", "www2.example.org" } }));
            Assert.AreEqual(80, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/", new Hash { { "HTTP_HOST", "www2.example.org:81" } }));
            Assert.AreEqual(81, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/",
                    new Hash { { "SERVER_NAME", "example.org" }, { "SERVER_PORT", "9292" } }));
            Assert.AreEqual(9292, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/",
                    new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org:9292" } }));
            Assert.AreEqual(9292, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/",
                    new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org" } }));
            Assert.AreEqual(80, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/",
                    new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org" }, { "HTTP_X_FORWARDED_SSL", "on" } }));
            Assert.AreEqual(443, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org" }, { "HTTP_X_FORWARDED_PROTO", "https" } }));
            Assert.AreEqual(443, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org" }, { "HTTP_X_FORWARDED_PORT", "9393" } }));
            Assert.AreEqual(9393, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org:9393" }, { "SERVER_PORT", "80" } }));
            Assert.AreEqual(9393, request.Port);

            request =
                new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org" }, { "SERVER_PORT", "9393" } }));
            Assert.AreEqual(80, request.Port);
        }

        [Test]
        public void Should_Figure_Out_The_Correct_Host_With_Port()
        {
            var request = new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "www2.example.org" } }));
            Assert.AreEqual("www2.example.org", request.HostWithPort);

            request = new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "localhost:81" } }));
            Assert.AreEqual("localhost:81", request.HostWithPort);

            request = new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "SERVER_NAME", "example.org" }, { "SERVER_PORT", "9292" } }));
            Assert.AreEqual("example.org:9292", request.HostWithPort);

            request = new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org:9292" } }));
            Assert.AreEqual("example.org:9292", request.HostWithPort);

            request = new Request(new MockRequest().EnvironmentFor("/",
                new Hash { { "HTTP_HOST", "localhost:81" }, { "HTTP_X_FORWARDED_HOST", "example.org" }, { "SERVER_PORT", "9393" } }));
            Assert.AreEqual("example.org", request.HostWithPort);
        }

        [Test]
        public void Should_Parse_The_Query_String()
        {
            var request = new Request(new MockRequest().EnvironmentFor("/?foo=bar&quux=bla"));

            var expected = new Dictionary<string, string> { { "foo", "bar" }, { "quux", "bla" } };
            CollectionAssert.AreEquivalent(expected, request.GET);
            Assert.AreEqual(0, request.POST.Count);
            Assert.AreEqual(expected, request.Params);
        }

        [Test]
        public void Should_Throw_If_Rack_Input_Is_Missing()
        {
            var request = new Request(new Dictionary<string, dynamic>());
            Assert.Throws<InvalidOperationException>(() => { var p = request.POST; });
        }

        [Test]
        public void Should_Parse_POST_Data_When_Method_Is_POST_And_No_Content_Type_Is_Given()
        {
            var request = new Request(new MockRequest().EnvironmentFor("/?foo=quux",
                new Hash { { "REQUEST_METHOD", "POST" }, { "input", "foo=bar&quux=bla" } }));

            Assert.IsNull(request.ContentType);
            Assert.IsNull(request.MediaType);
            Assert.AreEqual("foo=quux", request.QueryString);
            CollectionAssert.AreEquivalent(new Dictionary<string, string> { { "foo", "quux" } }, request.GET);
            CollectionAssert.AreEquivalent(new Dictionary<string, string> { { "foo", "bar" }, { "quux", "bla" } },
                                           request.POST);
        }
    }
}