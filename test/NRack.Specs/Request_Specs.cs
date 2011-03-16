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
            Assert.AreEqual(string.Empty, request.ScriptName);
            Assert.AreEqual("/", request.PathInfo);
            Assert.AreEqual(string.Empty, request.QueryString);
            Assert.AreEqual("example.com", request.Host);
            Assert.AreEqual(8080, request.Port);
            Assert.AreEqual(0, request.ContentLength);
            Assert.IsNull(request.ContentType);
        }
    }
}