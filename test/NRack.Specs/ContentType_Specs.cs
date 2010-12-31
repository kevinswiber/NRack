using System.Collections.Generic;
using System.Linq;
using NRack.Adapters;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class ContentType_Specs
    {
        [Test]
        public void Should_Set_Content_Type_To_Default_Text_Html_If_None_Is_Set()
        {
            var app = DetachedApplication.Create(env => new dynamic[] {200, new Hash(), "Hello, World"});
            
            var response = new ContentType(app).Call(new Dictionary<string, dynamic>());
            var headers = response[1];

            Assert.AreEqual("text/html", headers["Content-Type"]);
        }

        [Test]
        public void Should_Set_Content_Type_To_Chosen_Default_If_None_Is_Set()
        {
            var app = DetachedApplication.Create(env => new dynamic[] { 200, new Hash(), "Hello, World" });

            var response = new ContentType(app, "application/octet-stream").Call(new Dictionary<string, dynamic>());
            var headers = response[1];

            Assert.AreEqual("application/octet-stream", headers["Content-Type"]);            
        }

        [Test]
        public void Should_Not_Change_Content_Type_If_It_Is_Already_Set()
        {
            var app =
                DetachedApplication.Create(
                    env => new dynamic[] {200, new Hash {{"Content-Type", "foo/bar"}}, "Hello, World"});

            var response = new ContentType(app).Call(new Dictionary<string, dynamic>());
            var headers = response[1];

            Assert.AreEqual("foo/bar", headers["Content-Type"]);  
        }

        [Test]
        public void Should_Detect_Content_Type_Case_Insensitive()
        {
            var app =
                DetachedApplication.Create(
                    env => new dynamic[] { 200, new Hash { { "CONTENT-Type", "foo/bar" } }, "Hello, World" });

            var response = new ContentType(app).Call(new Dictionary<string, dynamic>());
            var headers = (Hash)response[1];

            var keyValuePair = headers.ToArray().Where(pair => pair.Key.ToLower() == "content-type").Single();

            Assert.AreEqual(new KeyValuePair<string, dynamic>("CONTENT-Type", "foo/bar"), keyValuePair); 
        }
    }
}