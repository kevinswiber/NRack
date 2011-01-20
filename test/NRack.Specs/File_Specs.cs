using System;
using System.IO;
using NRack.Extensions;
using NRack.Mock;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class File_Specs
    {
        private string _docRoot = AppDomain.CurrentDomain.BaseDirectory;

        [Test]
        public void Should_Serve_Files()
        {
            var response = new MockRequest(new File(_docRoot)).Get("/cgi/test");

            Assert.AreEqual(200, response.Status);
        }

        [Test]
        public void Should_Set_Last_Modified_Header()
        {
            var response = new MockRequest(new File(_docRoot)).Get("/cgi/test");

            var path = Path.Combine(_docRoot, @"cgi\test");
            var fileInfo = new FileInfo(path);

            Assert.AreEqual(200, response.Status);
            Assert.AreEqual(fileInfo.LastWriteTimeUtc.ToHttpDateString(), response["Last-Modified"]);
        }

        [Test]
        public void Should_Serve_Files_With_Url_Encoded_Filenames()
        {
            var response = new MockRequest(new File(_docRoot)).Get("/cgi/%74%65%73%74");

            Assert.AreEqual(200, response.Status);
        }

        [Test]
        public void Should_Not_Allow_Directory_Traversal()
        {
            var response = new MockRequest(new File(_docRoot)).Get("/cgi/../test");

            Assert.AreEqual(403, response.Status);
        }

        [Test]
        public void Should_Not_Allow_Directory_Traversal_With_Encoded_Periods()
        {
            var response = new MockRequest(new File(_docRoot)).Get("/%2E%2E/README");

            Assert.AreEqual(403, response.Status);
        }

        [Test]
        public void Should_Return_404_If_Cannot_Find_File()
        {
            var response = new MockRequest(new File(_docRoot)).Get("/cgi/blubb");

            Assert.AreEqual(404, response.Status);
        }

        [Test]
        public void Should_Detect_SystemCallErrors()
        {
            var response = new MockRequest(new File(_docRoot)).Get("/cgi");

            Assert.AreEqual(404, response.Status);
        }

        [Test]
        public void Should_Return_Bodies_That_Respond_To_ToPath()
        {
            var env = new MockRequest().EnvironmentFor("/cgi/test");
            var response = new File(_docRoot).Call(env);

            var path = Path.Combine(_docRoot, "cgi/test");

            Assert.AreEqual(200, response[0]);
            Assert.IsTrue(response[2] is IPathConvertible);
            Assert.AreEqual(path, response[2].ToPath());
        }

        [Test]
        public void Should_Return_Correct_Byte_Range_In_Body()
        {
            var env = new MockRequest().EnvironmentFor("/cgi/test");
            env["HTTP_RANGE"] = "bytes=22-33";
            var response = new MockResponse(new File(_docRoot).Call(env));

            Assert.AreEqual(206, response.Status);
            Assert.AreEqual("12", response["Content-Length"]);
            Assert.AreEqual("bytes 22-33/193", response["Content-Range"]);
            Assert.AreEqual("-*- test -*-", response.Body.ToString());
        }

        [Test]
        public void Should_Return_Error_For_Unsatisfiable_Byte_Range()
        {
            var env = new MockRequest().EnvironmentFor("/cgi/test");
            env["HTTP_RANGE"] = "bytes=1234-5678";
            var response = new MockResponse(new File(_docRoot).Call(env));

            Assert.AreEqual(416, response.Status);
            Assert.AreEqual("bytes */193", response["Content-Range"]);
        }
    }
}