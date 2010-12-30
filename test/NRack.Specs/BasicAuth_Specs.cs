using System;
using System.Collections.Generic;
using System.Text;
using NRack.Adapters;
using NRack.Auth;
using NRack.Mock;
using NUnit.Core;
using NUnit.Framework;

namespace NRack.Specs
{
    public class BasicAuth_Specs
    {
        private MockRequest _request;

        public string Realm { get { return "WallysWorld"; } }
        
        public dynamic UnprotectedApp 
        {
            get
            {
                return
                    ApplicationFactory.Create(
                        env =>
                        new[] {200, new Hash {{"Content-Type", "text/plain"}}, "Hi " + env["REMOTE_USER"]});
            }
        }

        public dynamic ProtectedApp
        {
            get
            {
                return new BasicAuthHandler(UnprotectedApp, (Func<string, string, bool>)((username, password) => username == "Boss"))
                           {
                               Realm = Realm
                           };
            }
        }

        [SetUp]
        public void TestSetup()
        {
            _request = new MockRequest(ProtectedApp);
        }

        private void RequestWithBasicAuth(string username, string password, Action<MockResponse> responseAction = null)
        {
            var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

            Request(new Dictionary<string, dynamic>() {{"HTTP_AUTHORIZATION", "Basic " + base64}}, responseAction);
        }

        private void Request(Action<MockResponse> responseAction)
        {
            Request(new Dictionary<string, dynamic>(), responseAction);
        }

        private void Request(Dictionary<string, dynamic> headers, Action<MockResponse> responseAction = null)
        {
            if (responseAction != null)
            {
                responseAction(_request.Get("/", headers));
            }
        }

        private void AssertBasicAuthChallenge(MockResponse response)
        {
            Assert.AreEqual(401, response.Status);
            Assert.IsTrue(response.Headers.ContainsKey("WWW-Authenticate"));
            Assert.IsTrue(response["WWW-Authenticate"].Contains("Basic realm=\"" + Realm + "\""));
            Assert.IsEmpty(response.Body.ToString());
        }

        [Test]
        public void Should_Challenge_Correctly_When_No_Credentials_Are_Specified()
        {
            Request(AssertBasicAuthChallenge);
        }

        [Test]
        public void Should_Rechallenge_If_Incorrect_Credentials_Are_Specified()
        {
            RequestWithBasicAuth("joe", "password", AssertBasicAuthChallenge);
        }

        [Test]
        public void Should_Return_Application_Output_If_Correct_Credentials_Are_Specified()
        {
            RequestWithBasicAuth("Boss", "password", response =>
                                                         {
                                                             Assert.AreEqual(200, response.Status);
                                                             Assert.AreEqual("Hi Boss", response.Body.ToString());
                                                         });
        }

        [Test]
        public void Should_Return_400_Bad_Request_If_Different_Auth_Scheme_Used()
        {
            Request(new Dictionary<string, dynamic> {{"HTTP_AUTHORIZATION", "Digest params"}}, 
                response =>
                    {
                        Assert.AreEqual(400, response.Status);
                        Assert.IsFalse(response.Headers.ContainsKey("WWW-Authenticate"));
                    });
        }

        [Test]
        public void Should_Take_Realm_As_Optional_Constructor_Arg()
        {
            var app = new BasicAuthHandler(UnprotectedApp, Realm, (Func<string, string, bool>)((username, password) => true));
            Assert.AreEqual(Realm, app.Realm);
        }
    }
}