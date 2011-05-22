using System.Collections.Generic;
using NRack.Helpers;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class Response_Specs
    {
        [Test]
        public void Should_Have_Sensible_Default_Values()
        {
            var response = new Response();
            var res = response.Finish();
            var status = res[0];
            var header = res[1];
            var body = (IIterable)res[2];

            Assert.AreEqual(200, status);
            CollectionAssert.AreEquivalent(new Hash { { "Content-Type", "text/html" } }, header);
            body.Each(part => Assert.AreEqual(string.Empty, part));
        }

        [Test]
        public void Can_Be_Written_To()
        {
            var response = new Response();

            var body = response.Finish(_ =>
            {
                response.Write("foo");
                response.Write("bar");
                response.Write("baz");
            })[2];
            var parts = new List<string>();
            ((IIterable)body).Each(part => parts.Add(part));

            CollectionAssert.AreEquivalent(new List<string> { "foo", "bar", "baz" }, parts);
        }

        [Test]
        public void Should_Not_Add_Or_Change_Content_Length_When_Finishing()
        {
            var response = new Response();
            response.Status = 200;
            response.Finish();
            Assert.IsFalse(response.Headers.ContainsKey("Content-Length"));

            response = new Response();
            response.Status = 200;
            response.Headers["Content-Length"] = "10";
            response.Finish();

            Assert.AreEqual("10", response.Headers["Content-Length"]);
        }

        [Test]
        public void Should_Update_Content_Length_When_Body_Appended_To_Using_Write()
        {
            var response = new Response();
            response.Status = 200;

            Assert.IsFalse(response.Headers.ContainsKey("Content-Length"));
            response.Write("Hi");
            Assert.AreEqual("2", response.Headers["Content-Length"]);

            response.Write(" there");
            Assert.AreEqual("8", response.Headers["Content-Length"]);
        }
    }
}