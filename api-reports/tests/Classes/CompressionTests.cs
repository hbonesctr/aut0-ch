using Xunit;
using openrmf_report_api.Classes;
using System;
using System.Text;

namespace tests.Classes
{
    public class CompressionTests
    {
        // ---- Pass Tests ----

        [Fact]
        public void Test_CompressStringReturnsNonEmptyResult()
        {
            string original = "Hello, OpenRMF!";
            string compressed = Compression.CompressString(original);
            Assert.NotNull(compressed);
            Assert.NotEmpty(compressed);
        }

        [Fact]
        public void Test_CompressAndDecompressRoundTrip()
        {
            string original = "This is a test string for compression and decompression round-trip.";
            string compressed = Compression.CompressString(original);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Test_CompressStringProducesBase64Output()
        {
            string original = "OpenRMF Report API Test";
            string compressed = Compression.CompressString(original);
            // Base64 should only contain valid characters
            byte[] buffer = Convert.FromBase64String(compressed);
            Assert.NotNull(buffer);
            Assert.True(buffer.Length > 0);
        }

        [Fact]
        public void Test_CompressStringDifferentFromOriginal()
        {
            string original = "SomeTextToCompress";
            string compressed = Compression.CompressString(original);
            Assert.NotEqual(original, compressed);
        }

        [Fact]
        public void Test_CompressLargeStringRoundTrip()
        {
            string original = new string('A', 10000);
            string compressed = Compression.CompressString(original);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Test_CompressJsonStringRoundTrip()
        {
            string original = "{\"systemGroupId\":\"sys001\",\"hostname\":\"host1\",\"severity\":4}";
            string compressed = Compression.CompressString(original);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Test_CompressStringWithSpecialCharactersRoundTrip()
        {
            string original = "Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";
            string compressed = Compression.CompressString(original);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Test_CompressStringWithUnicodeRoundTrip()
        {
            string original = "Unicode: \u00e9\u00e0\u00fc\u00f1";
            string compressed = Compression.CompressString(original);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(original, decompressed);
        }

        // ---- Fail / Negative Tests ----

        [Fact]
        public void Test_DecompressStringThrowsOnInvalidBase64()
        {
            Assert.ThrowsAny<Exception>(() => Compression.DecompressString("!!!not-valid-base64!!!"));
        }

        [Fact]
        public void Test_DecompressStringThrowsOnEmptyString()
        {
            Assert.ThrowsAny<Exception>(() => Compression.DecompressString(""));
        }

        [Fact]
        public void Test_CompressedOutputIsNotEqualToDecompressed()
        {
            string original = "Some text";
            string compressed = Compression.CompressString(original);
            Assert.NotEqual(original, compressed);
        }
    }
}
