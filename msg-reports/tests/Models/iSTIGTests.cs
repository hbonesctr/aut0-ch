using Xunit;
using openrmf_msg_report.Models;
using System.Collections.Generic;

namespace tests.Models
{
    public class iSTIGTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewiSTIG_IsNotNull()
        {
            var iStig = new iSTIG();
            Assert.NotNull(iStig);
        }

        [Fact]
        public void Test_NewiSTIG_STIG_INFOIsInitialized()
        {
            var iStig = new iSTIG();
            Assert.NotNull(iStig.STIG_INFO);
        }

        [Fact]
        public void Test_NewiSTIG_VULNListIsInitializedAndEmpty()
        {
            var iStig = new iSTIG();
            Assert.NotNull(iStig.VULN);
            Assert.Empty(iStig.VULN);
        }

        [Fact]
        public void Test_iSTIG_VULNList_CanAddItems()
        {
            var iStig = new iSTIG();
            iStig.VULN.Add(new VULN { STATUS = "NotAFinding" });
            iStig.VULN.Add(new VULN { STATUS = "Open" });

            Assert.Equal(2, iStig.VULN.Count);
        }

        [Fact]
        public void Test_iSTIG_STIG_INFO_SI_DATAIsAccessible()
        {
            var iStig = new iSTIG();
            iStig.STIG_INFO.SI_DATA.Add(new SI_DATA { SID_NAME = "version", SID_DATA = "1" });

            Assert.Single(iStig.STIG_INFO.SI_DATA);
            Assert.Equal("version", iStig.STIG_INFO.SI_DATA[0].SID_NAME);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_iSTIG_VULNList_IsNotNullWhenEmpty()
        {
            var iStig = new iSTIG();
            // The VULN list must be an initialized list, never null
            Assert.NotNull(iStig.VULN);
        }

        [Fact]
        public void Test_iSTIG_VULNList_CountIsZeroWhenNotPopulated()
        {
            var iStig = new iSTIG();
            Assert.Equal(0, iStig.VULN.Count);
        }
    }
}
