using System;
using System.Collections.Generic;
using NRack.Helpers;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class ContentLength_Specs
    {
        [Test]
        public void Should_Set_Content_Length_On_Array_Bodies_If_None_Is_Set()
        {
            var app =
                DetachedApplication.Create(
                    env =>
                    new dynamic[]
                        {200, new Hash {{"Content-Type", "text/plain"}}, "Hello, World!"});

            var response = new ContentLength(app).Call(new Dictionary<string, dynamic>());

            Assert.AreEqual("13", response[1]["Content-Length"]);
        }

        [Test]
        public void Should_Not_Set_Content_Length_On_Variable_Length_Bodies()
        {
            Func<string> body = () => "Hello World!";
            var iterableBody = new IterableAdapter(new Proc(body), lambda => lambda.Call());

            var app =
                DetachedApplication.Create(
                    env => new dynamic[] {200, new Hash {{"Content-Type", "text/plain"}}, iterableBody});

            var response = new ContentLength(app).Call(new Dictionary<string, dynamic>());

            Assert.IsFalse(response[1].ContainsKey("Content-Length"));
        }

        [Test]
        public void Should_Not_Change_Content_Length_If_It_Is_Already_Set()
        {
            var app =
                DetachedApplication.Create(
                    env =>
                    new dynamic[]
                        {200, new Hash {{"Content-Type", "text/plain"}, {"Content-Length", "1"}}, "Hello, World!"});

            var response = new ContentLength(app).Call(new Dictionary<string, dynamic>());

            Assert.AreEqual("1", response[1]["Content-Length"]);
        }

        [Test]
        public void Should_Not_Set_Content_Length_On_304_Responses()
        {
            var app =
                DetachedApplication.Create(
                    env => new dynamic[] {304, new Hash {{"Content-Type", "text/plain"}}, string.Empty});

            var response = new ContentLength(app).Call(new Dictionary<string, dynamic>());

            Assert.IsFalse(response[1].ContainsKey("Content-Length"));
        }

        [Test]
        public void Should_Not_Set_Content_Length_When_Transfer_Encoding_Is_Chunked()
        {
            var app =
                DetachedApplication.Create(
                    env => new dynamic[] {200, new Hash {{"Transfer-Encoding", "chunked"}}, string.Empty});

            var response = new ContentLength(app).Call(new Dictionary<string, dynamic>());

            Assert.IsFalse(response[1].ContainsKey("Content-Length"));
        }
    }
}