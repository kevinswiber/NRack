using System;
using NRack.ServerHelpers;
using NUnit.Framework;

namespace NRack.Specs
{
    public class ServerOptionsBuilder_Specs
    {
        [Test]
        public void Should_Parse_Arguments_Into_Server_Options()
        {
            var args = new[] {"--host", "localhost", "-p", "8080"};

            var serverOptions = new ServerOptions();

            ServerOptionsBuilder.BuildFromArgs(args, serverOptions);

            Assert.AreEqual("localhost",serverOptions.Host);
            Assert.AreEqual("8080", serverOptions.Port);
        }

        [Test]
        public void Should_Parse_Config_Type_From_Arguments()
        {
            const string argString = "--host localhost -p 8080 NRack.Specs.TestConfig,NRack.Specs";
            var args = argString.Split(new[] {' '});

            var serverOptions = new ServerOptions();

            ServerOptionsBuilder.BuildFromArgs(args, serverOptions);

            Assert.AreEqual(Type.GetType("NRack.Specs.TestConfig,NRack.Specs"), serverOptions.Config);
        }
        
    }

    public class TestConfig
    {
        public TestConfig()
        { }
    }
}
