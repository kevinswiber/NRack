using NRack.Adapters;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class HeaderHash_Specs
    {
        [Test]
        public void Should_Retain_Header_Case()
        {
            var headers = new HeaderHash(new Hash {{"Content-MD5", "d5ff4e2a0 ..."}});

            headers["ETag"] = "Boo!";

            Assert.AreEqual(new Hash {{"Content-MD5", "d5ff4e2a0 ..."}, {"ETag", "Boo!"}}, headers);
        }

        [Test]
        public void Should_Check_Existence_of_Keys_Case_Insensitively()
        {
            var headers = new HeaderHash(new Hash {{"Content-MD5", "d5ff4e2a0 ..."}});

            Assert.IsTrue(headers.ContainsKey("content-md5"));
            Assert.IsFalse(headers.ContainsKey("ETag"));
        }

        [Test]
        public void Should_Merge_Case_Insensitively()
        {
            var headers = new HeaderHash(new Hash {{"ETag", "HELLO"}, {"content-length", "123"}});

            var otherHash = new Hash {{"Etag", "WORLD"}, {"Content-Length", "321"}, {"Foo", "BAR"}};
            
            var merged = headers.Merge(otherHash);

            Assert.AreEqual(otherHash, merged);
        }

        [Test]
        public void Should_Overwrite_Case_Insensitively_And_Assume_The_New_Keys_Case()
        {
            var headers = new HeaderHash(new Hash {{"Foo-Bar", "baz"}});

            headers["foo-bar"] = "bizzle";

            Assert.AreEqual("bizzle", headers["FOO-BAR"]);
            Assert.AreEqual(1, headers.Count);
            Assert.AreEqual(new Hash{{"foo-bar", "bizzle"}}, headers.ToHash());
        }

        [Test]
        public void Should_Be_Converted_To_Real_Hash()
        {
            var headers = new HeaderHash(new Hash {{"foo", "bar"}});
            Assert.NotNull(headers as Hash);
        }

        [Test]
        public void Should_Convert_Array_Values_To_Strings_When_Converting_To_Hash()
        {
            var headers = new HeaderHash(new Hash { { "foo", new[] { "bar", "baz" } } });
            Assert.AreEqual(new Hash {{"foo", "bar\nbaz"}}, headers.ToHash());
        }

        [Test]
        public void Should_Replace_Hashes_Correctly()
        {
            var headers = new HeaderHash(new Hash {{"Foo-Bar", "baz"}});
            var hash = new Hash {{"foo", "bar"}};
            
            headers.Replace(hash);

            Assert.AreEqual("bar", headers["foo"]);
        }

        [Test]
        public void Should_Be_Able_To_Delete_The_Given_Key_Case_Sensitively()
        {
            var headers = new HeaderHash(new Hash { { "foo", "bar" } });

            headers.Delete("foo");

            Assert.IsFalse(headers.ContainsKey("foo"));
            Assert.IsFalse(headers.ContainsKey("FOO"));
        }

        [Test]
        public void Should_Be_Able_To_Delete_The_Given_Key_Case_Insensitively()
        {
            var headers = new HeaderHash(new Hash { { "foo", "bar" } });

            headers.Remove("FOO");

            Assert.IsFalse(headers.ContainsKey("foo"));
            Assert.IsFalse(headers.ContainsKey("FOO"));
        }

        [Test]
        public void Should_Return_The_Deleted_Value_When_Delete_Is_Called_On_An_Existing_Key()
        {
            var headers = new HeaderHash(new Hash { { "foo", "bar" } });

            Assert.AreEqual("bar", headers.Delete("Foo"));
        }

        [Test]
        public void Should_Return_Null_When_Delete_Is_Called_On_A_Non_Existent_Key()
        {
            var headers = new HeaderHash(new Hash { { "foo", "bar" } });

            Assert.Null(headers.Delete("Hello"));            
        }

        [Test]
        public void Should_Avoid_Unnecessary_Object_Creation_If_Possible()
        {
            var a = new HeaderHash(new Hash { { "foo", "bar" } });
            var b = HeaderHash.Create(a);

            Assert.AreEqual(b.GetHashCode(), a.GetHashCode());
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Should_Convert_Array_Values_To_Strings_When_Responding_To_Each()
        {
            var headers = new HeaderHash(new Hash { { "foo", new[] { "bar", "baz" } } });

            headers.Each((key, value) =>
                             {
                                 Assert.AreEqual("foo", key);
                                 Assert.AreEqual("bar\r\n\tbaz", value);
                             });
        }
    }
}