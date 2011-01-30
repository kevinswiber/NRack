using NRack.Mock;
using NUnit.Framework;

namespace NRack.Specs
{
    public class MockRequest_Specs
    {
        public void Should_Use_All_Parts_Of_An_URL()
        {
            var env = new MockRequest()
                .EnvironmentFor("https://bla.example.org:9292/meh/foo?bar");

            Assert.AreEqual("GET", env["REQUEST_METHOD"]);
            Assert.AreEqual("bla.example.org", env["SERVER_NAME"]);
            Assert.AreEqual("9292", env["SERVER_PORT"]);
            Assert.AreEqual("bar", env["QUERY_STRING"]);
            Assert.AreEqual("/meh/foo", env["PATH_INFO"]);
            Assert.AreEqual("https", env["rack.url_scheme"]);
        }
    }
}