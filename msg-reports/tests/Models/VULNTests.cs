using Xunit;
using openrmf_msg_report.Models;
using System.Collections.Generic;

namespace tests.Models
{
    public class VULNTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewVULN_IsNotNull()
        {
            var v = new VULN();
            Assert.NotNull(v);
        }

        [Fact]
        public void Test_NewVULN_STIG_DATAListIsInitialized()
        {
            var v = new VULN();
            Assert.NotNull(v.STIG_DATA);
            Assert.Empty(v.STIG_DATA);
        }

        [Fact]
        public void Test_VULN_AllStatusFields_AreSet()
        {
            var v = new VULN
            {
                STATUS = "NotAFinding",
                FINDING_DETAILS = "No issue found.",
                COMMENTS = "Reviewed by auditor.",
                SEVERITY_OVERRIDE = "low",
                SEVERITY_JUSTIFICATION = "Mitigated by compensating control."
            };

            Assert.Equal("NotAFinding", v.STATUS);
            Assert.Equal("No issue found.", v.FINDING_DETAILS);
            Assert.Equal("Reviewed by auditor.", v.COMMENTS);
            Assert.Equal("low", v.SEVERITY_OVERRIDE);
            Assert.Equal("Mitigated by compensating control.", v.SEVERITY_JUSTIFICATION);
        }

        [Fact]
        public void Test_VULN_STIG_DATA_CanAddItems()
        {
            var v = new VULN();
            v.STIG_DATA.Add(new STIG_DATA { VULN_ATTRIBUTE = "Vuln_Num", ATTRIBUTE_DATA = "V-220697" });
            v.STIG_DATA.Add(new STIG_DATA { VULN_ATTRIBUTE = "Severity", ATTRIBUTE_DATA = "high" });

            Assert.Equal(2, v.STIG_DATA.Count);
            Assert.Equal("V-220697", v.STIG_DATA[0].ATTRIBUTE_DATA);
        }

        [Theory]
        [InlineData("Open")]
        [InlineData("NotAFinding")]
        [InlineData("Not_Applicable")]
        [InlineData("Not_Reviewed")]
        public void Test_VULN_STATUS_AcceptsKnownValues(string status)
        {
            var v = new VULN { STATUS = status };
            Assert.Equal(status, v.STATUS);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewVULN_StatusIsNullByDefault()
        {
            var v = new VULN();
            Assert.Null(v.STATUS);
        }

        [Fact]
        public void Test_VULN_STATUS_DoesNotMatchWrongValue()
        {
            var v = new VULN { STATUS = "NotAFinding" };
            Assert.NotEqual("Open", v.STATUS);
        }

        [Fact]
        public void Test_NewVULN_FindingDetailsIsNullByDefault()
        {
            var v = new VULN();
            Assert.Null(v.FINDING_DETAILS);
        }
    }
}
