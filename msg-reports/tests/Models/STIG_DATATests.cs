using Xunit;
using openrmf_msg_report.Models;

namespace tests.Models
{
    public class STIG_DATATests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewSTIG_DATA_IsNotNull()
        {
            var data = new STIG_DATA();
            Assert.NotNull(data);
        }

        [Fact]
        public void Test_STIG_DATA_Properties_AreSetCorrectly()
        {
            var data = new STIG_DATA
            {
                VULN_ATTRIBUTE = "Vuln_Num",
                ATTRIBUTE_DATA = "V-220697"
            };

            Assert.Equal("Vuln_Num", data.VULN_ATTRIBUTE);
            Assert.Equal("V-220697", data.ATTRIBUTE_DATA);
        }

        [Theory]
        [InlineData("Vuln_Num", "V-220697")]
        [InlineData("Severity", "high")]
        [InlineData("Check_Content", "Check this setting.")]
        [InlineData("Vuln_Discuss", "This vulnerability poses a risk.")]
        public void Test_STIG_DATA_KnownAttributePairs_AreStoredCorrectly(string attr, string attrData)
        {
            var data = new STIG_DATA { VULN_ATTRIBUTE = attr, ATTRIBUTE_DATA = attrData };
            Assert.Equal(attr, data.VULN_ATTRIBUTE);
            Assert.Equal(attrData, data.ATTRIBUTE_DATA);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewSTIG_DATA_VULN_ATTRIBUTEIsNullByDefault()
        {
            var data = new STIG_DATA();
            Assert.Null(data.VULN_ATTRIBUTE);
        }

        [Fact]
        public void Test_NewSTIG_DATA_ATTRIBUTE_DATAIsNullByDefault()
        {
            var data = new STIG_DATA();
            Assert.Null(data.ATTRIBUTE_DATA);
        }

        [Fact]
        public void Test_STIG_DATA_VULN_ATTRIBUTE_DoesNotMatchWrongValue()
        {
            var data = new STIG_DATA { VULN_ATTRIBUTE = "Vuln_Num" };
            Assert.NotEqual("Severity", data.VULN_ATTRIBUTE);
        }
    }
}
