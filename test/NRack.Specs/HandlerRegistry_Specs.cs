using System;
using NRack.Hosting.Kayak;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class HandlerRegistry_Specs
    {
        public class LobsterHandler
        {}

        [Test]
        public void Should_Have_Registered_Default_Handlers()
        {
            Assert.AreEqual(typeof (KayakHandler), HandlerRegistry.Get("kayak"));
        }

        [Test]
        public void Should_Throw_Argument_Exception_If_Handler_Does_Not_Exist()
        {
            Assert.Throws(typeof (ArgumentException), () => HandlerRegistry.Get("boom"));
        }

        [Test]
        public void Should_Register_Custom_Handler()
        {
            HandlerRegistry.Register("rock_lobster", typeof (LobsterHandler));

            Assert.AreEqual(typeof (LobsterHandler), HandlerRegistry.Get("rock_lobster"));
        }
    }
}