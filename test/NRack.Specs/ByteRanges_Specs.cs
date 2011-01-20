using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class ByteRanges_Specs
    {
        [Test]
        public void Should_Ignore_Syntactically_Invalid_Byte_Ranges()
        {
            Assert.IsNull(Utils.ByteRanges(new Dictionary<string, dynamic>(), 500));
            Assert.IsNull(CreateByteRanges("foobar", 500));
            Assert.IsNull(CreateByteRanges("furlongs=123-456", 500));
            Assert.IsNull(CreateByteRanges("bytes=", 500));
            Assert.IsNull(CreateByteRanges("bytes=-", 500));
            Assert.IsNull(CreateByteRanges("bytes=123,456", 500));

            // A range of non-positive length is syntactically invalid and ignored:
            Assert.IsNull(CreateByteRanges("bytes=456-123", 500));
            Assert.IsNull(CreateByteRanges("bytes=456-455", 500));
        }

        [Test]
        public void Should_Parse_Simple_Byte_Ranges()
        {
            Assert.AreEqual(new[] {Utils.CreateRangeArray(123, 456)}, CreateByteRanges("bytes=123-456", 500));
            Assert.AreEqual(new[] {Utils.CreateRangeArray(123, 499)}, CreateByteRanges("bytes=123-", 500));
            Assert.AreEqual(new[] {Utils.CreateRangeArray(400, 499)}, CreateByteRanges("bytes=-100", 500));
            Assert.AreEqual(new[] {Utils.CreateRangeArray(0, 0)}, CreateByteRanges("bytes=0-0", 500));
            Assert.AreEqual(new[] {Utils.CreateRangeArray(499, 499)}, CreateByteRanges("bytes=499-499", 500));
        }

        [Test]
        public void Should_Truncate_Byte_Ranges()
        {
            Assert.AreEqual(new[] {Utils.CreateRangeArray(123, 499)}, CreateByteRanges("bytes=123-999", 500));
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=600-999", 500));
            Assert.AreEqual(new[] {Utils.CreateRangeArray(0, 499)}, CreateByteRanges("bytes=-999", 500));
        }

        [Test]
        public void Should_Ignore_Unsatisfiable_Byte_Ranges()
        {
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=500-", 0));
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=999-", 0));
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=500-501", 0));
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=-0", 0));
        }

        [Test]
        public void Should_Handle_Byte_Ranges_Of_Empty_Files()
        {
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=123-456", 0));
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=0-", 0));
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=-100", 0));
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=0-0", 0));
            Assert.AreEqual(new long[] { }, CreateByteRanges("bytes=-0", 0));
        }
        
        private IEnumerable<IEnumerable<long>> CreateByteRanges(string rangeString, int size)
        {
            return Utils.ByteRanges(new Dictionary<string, dynamic> {{"HTTP_RANGE", rangeString}}, size);
        }
    }
}