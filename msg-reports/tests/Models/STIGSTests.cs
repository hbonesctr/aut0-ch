using Xunit;
using openrmf_msg_report.Models;

namespace tests.Models
{
    public class STIGSTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewSTIGS_IsNotNull()
        {
            var stigs = new STIGS();
            Assert.NotNull(stigs);
        }

        [Fact]
        public void Test_NewSTIGS_iSTIGIsInitialized()
        {
            var stigs = new STIGS();
            Assert.NotNull(stigs.iSTIG);
        }

        [Fact]
        public void Test_STIGS_iSTIG_VULNListIsInitialized()
        {
            var stigs = new STIGS();
            Assert.NotNull(stigs.iSTIG.VULN);
        }

        [Fact]
        public void Test_STIGS_iSTIG_CanBeReplaced()
        {
            var stigs = new STIGS();
            var newISTIG = new iSTIG();
            newISTIG.VULN.Add(new VULN { STATUS = "NotAFinding" });
            stigs.iSTIG = newISTIG;

            Assert.Single(stigs.iSTIG.VULN);
            Assert.Equal("NotAFinding", stigs.iSTIG.VULN[0].STATUS);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewSTIGS_iSTIG_VULNCountIsZeroWhenNotPopulated()
        {
            var stigs = new STIGS();
            Assert.Equal(0, stigs.iSTIG.VULN.Count);
        }

        [Fact]
        public void Test_NewSTIGS_iSTIG_STIG_INFOIsInitialized()
        {
            var stigs = new STIGS();
            Assert.NotNull(stigs.iSTIG.STIG_INFO);
        }
    }
}
