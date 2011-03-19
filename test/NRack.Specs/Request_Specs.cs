using System.Collections.Generic;
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
            Assert.AreEqual(string.Empty, request.ScriptName);
            Assert.AreEqual("/", request.PathInfo);
            Assert.AreEqual(string.Empty, request.QueryString);
            Assert.AreEqual("example.com", request.Host);
            Assert.AreEqual(8080, request.Port);
            Assert.AreEqual(0, request.ContentLength);
            Assert.IsNull(request.ContentType);
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
    }
}