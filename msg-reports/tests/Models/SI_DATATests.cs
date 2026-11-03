using Xunit;
using openrmf_msg_report.Models;

namespace tests.Models
{
    public class SI_DATATests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewSI_DATA_IsNotNull()
        {
            var data = new SI_DATA();
            Assert.NotNull(data);
        }

        [Fact]
        public void Test_SI_DATA_Properties_AreSetCorrectly()
        {
            var data = new SI_DATA { SID_NAME = "version", SID_DATA = "1" };

            Assert.Equal("version", data.SID_NAME);
            Assert.Equal("1", data.SID_DATA);
        }

        [Theory]
        [InlineData("version", "1")]
        [InlineData("title", "Windows 10 STIG")]
        [InlineData("releaseinfo", "Release: 23 Benchmark Date: 26 Jan 2023")]
        public void Test_SI_DATA_KnownStigInfoPairs_AreStoredCorrectly(string name, string dataVal)
        {
            var data = new SI_DATA { SID_NAME = name, SID_DATA = dataVal };
            Assert.Equal(name, data.SID_NAME);
            Assert.Equal(dataVal, data.SID_DATA);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewSI_DATA_SID_NAMEIsNullByDefault()
        {
            var data = new SI_DATA();
            Assert.Null(data.SID_NAME);
        }

        [Fact]
        public void Test_NewSI_DATA_SID_DATAIsNullByDefault()
        {
            var data = new SI_DATA();
            Assert.Null(data.SID_DATA);
        }

        [Fact]
        public void Test_SI_DATA_SID_NAME_DoesNotMatchWrongValue()
        {
            var data = new SI_DATA { SID_NAME = "version" };
            Assert.NotEqual("title", data.SID_NAME);
        }
    }
}
