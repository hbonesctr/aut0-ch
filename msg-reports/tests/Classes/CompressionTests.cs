using Xunit;
using openrmf_msg_report.Classes;

namespace tests.Classes
{
    public class CompressionTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_CompressString_ReturnsNonEmptyString()
        {
            string original = "Hello, OpenRMF!";
            string compressed = Compression.CompressString(original);
            Assert.NotNull(compressed);
            Assert.NotEmpty(compressed);
        }

        [Fact]
        public void Test_CompressAndDecompress_RoundTrip_ReturnsOriginalString()
        {
            string original = "This is a test string for compression.";
            string compressed = Compression.CompressString(original);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Test_CompressAndDecompress_LargeString_RoundTrip()
        {
            string original = new string('A', 10_000);
            string compressed = Compression.CompressString(original);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Test_CompressString_OutputDiffersFromInput()
        {
            string original = "Some meaningful text that should compress differently.";
            string compressed = Compression.CompressString(original);
            Assert.NotEqual(original, compressed);
        }

        [Fact]
        public void Test_CompressAndDecompress_XmlPayload_RoundTrip()
        {
            string xml = "<CHECKLIST><ASSET><HOST_NAME>TESTHOST</HOST_NAME></ASSET></CHECKLIST>";
            string compressed = Compression.CompressString(xml);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(xml, decompressed);
        }

        [Theory]
        [InlineData("short")]
        [InlineData("A somewhat longer string with spaces and punctuation!")]
        [InlineData("1234567890")]
        public void Test_CompressAndDecompress_VariousStrings_RoundTrip(string input)
        {
            string compressed = Compression.CompressString(input);
            string decompressed = Compression.DecompressString(compressed);
            Assert.Equal(input, decompressed);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_CompressString_TwoIdenticalInputs_ProduceSameOutput()
        {
            string input = "Repeatable compression output";
            string c1 = Compression.CompressString(input);
            string c2 = Compression.CompressString(input);
            Assert.Equal(c1, c2);
        }

        [Fact]
        public void Test_CompressString_DifferentInputs_ProduceDifferentOutput()
        {
            string c1 = Compression.CompressString("String one");
            string c2 = Compression.CompressString("String two");
            Assert.NotEqual(c1, c2);
        }

        [Fact]
        public void Test_DecompressString_InvalidInput_ThrowsException()
        {
            Assert.Throws<System.FormatException>(() =>
                Compression.DecompressString("this-is-not-valid-base64!!"));
        }
    }
}
