using System;
using NRack.Mock;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class LambdaLobster_Specs
    {
        [Test]
        public void Should_Look_Like_A_Lobster()
        {
            var response = new MockRequest(Lobster.LambdaLobster).Get("/");

            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.Body.ToString().Contains("(,(,,(,,,("));
            Assert.IsTrue(response.Body.ToString().Contains("?flip"));
        }

        [Test]
        public void Should_Be_Flippable()
        {
            var response = new MockRequest(Lobster.LambdaLobster).Get("/?flip");

            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.Body.ToString().Contains("(,,,(,,(,("));
        }
    }

    public class Lobster_Specs
    {
        [Test]
        public void Should_Look_Like_A_Lobster()
        {
            var response = new MockRequest(new Lobster()).Get("/");

            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.Body.ToString().Contains("(,(,,(,,,("));
            Assert.IsTrue(response.Body.ToString().Contains("?flip"));
            Assert.IsTrue(response.Body.ToString().Contains("crash"));
        }

        [Test]
        public void Should_Be_Flippable()
        {
            var response = new MockRequest(new Lobster()).Get("/?flip=left");

            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.Body.ToString().Contains("(,,,(,,(,("));
        }

        [Test]
        public void Should_Provide_Crashing_For_Test_Purposes()
        {
            Assert.Throws<InvalidOperationException>(() => new MockRequest(new Lobster()).Get("/?flip=crash"));
        }
    }
}