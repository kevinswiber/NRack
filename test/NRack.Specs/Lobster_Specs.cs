using NRack.Mock;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class Lobster_Specs
    {
        [Test]
        public void Should_Look_Like_A_Lobster()
        {
            var response = new MockRequest(Lobster.LambdaLobster).Get("/");

            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.Body.ToString().Contains("(,(,,(,,,("));
            Assert.IsTrue(response.Body.ToString().Contains("?flip"));
            /*
             *     res.should.be.ok
    res.body.should.include "(,(,,(,,,("
    res.body.should.include "?flip"
             */
        }
    }
}