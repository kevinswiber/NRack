using NRack.Helpers;
using NRack.ServerHelpers;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class Server_Specs
    {
        [Test]
        public void Should_Override_Config_If_App_Is_Passed_In()
        {
            var server = new Server(new Hash {{ServerOptionKeys.App, "FOO"}});

            Assert.AreEqual(server.App, "FOO");
        }
    }
}