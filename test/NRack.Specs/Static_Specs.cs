using System;
using System.Collections.Generic;
using NRack.Helpers;
using NRack.Mock;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class Static_Specs
    {
        private string _root = AppDomain.CurrentDomain.BaseDirectory;
        private Hash _options;
        private Hash _hashOptions;

        private MockRequest _request;
        private MockRequest _hashRequest;
        
        public Static_Specs()
        {
            _options = new Hash {{"urls", new[] {"/cgi"}}, {"root", _root}};
            _hashOptions = new Hash {{"urls", new Hash {{"/cgi/sekret", "/cgi/test"}}}, {"root", _root}};

            _request = new MockRequest(new Static(new DummyApp(), _options));
            _hashRequest = new MockRequest(new Static(new DummyApp(), _hashOptions));
        }

        [Test]
        public void Should_Serve_Files()
        {
            var response = _request.Get("/cgi/test");
            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.Body.ToString().Contains("test"));
        }

        [Test]
        public void Should_Return_404_If_Url_Root_Is_Known_But_Cannot_Find_The_File()
        {
            var response = _request.Get("/cgi/foo");
            Assert.AreEqual(404, response.Status);
        }

        [Test]
        public void Should_Call_Down_The_Chain_If_Url_Root_Is_Not_Known()
        {
            var response = _request.Get("/something/else");
            Assert.AreEqual(200, response.Status);
            Assert.AreEqual("Hello World", response.Body.ToString());
        }

        [Test]
        public void Should_Serve_Hidden_Files()
        {
            var response = _hashRequest.Get("/cgi/sekret");

            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.Body.ToString().Contains("test"));
        }

        [Test]
        public void Should_Call_Down_The_Chain_If_Uri_Is_Not_Specified()
        {
            var response = _hashRequest.Get("/something/else");

            Assert.AreEqual(200, response.Status);
            Assert.AreEqual("Hello World", response.Body.ToString());
        }
    }

    public class DummyApp : ICallable
    {
        #region Implementation of ICallable

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            return new dynamic[] {200, new Hash(), "Hello World"};
        }

        #endregion
    }
}