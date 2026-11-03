using Xunit;
using openrmf_msg_report.Models;

namespace tests.Models
{
    public class CHECKLISTTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewCHECKLIST_IsNotNull()
        {
            var chk = new CHECKLIST();
            Assert.NotNull(chk);
        }

        [Fact]
        public void Test_NewCHECKLIST_ASSETIsInitialized()
        {
            var chk = new CHECKLIST();
            Assert.NotNull(chk.ASSET);
        }

        [Fact]
        public void Test_NewCHECKLIST_STIGSIsInitialized()
        {
            var chk = new CHECKLIST();
            Assert.NotNull(chk.STIGS);
        }

        [Fact]
        public void Test_CHECKLIST_AssignAsset_IsCorrect()
        {
            var asset = new ASSET { HOST_NAME = "TESTHOST", HOST_IP = "10.0.0.5" };
            var chk = new CHECKLIST { ASSET = asset };

            Assert.Equal("TESTHOST", chk.ASSET.HOST_NAME);
            Assert.Equal("10.0.0.5", chk.ASSET.HOST_IP);
        }

        [Fact]
        public void Test_CHECKLIST_AssignSTIGS_IsCorrect()
        {
            var stigs = new STIGS();
            var chk = new CHECKLIST { STIGS = stigs };
            Assert.NotNull(chk.STIGS);
            Assert.NotNull(chk.STIGS.iSTIG);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_CHECKLIST_ASSET_HostNameIsNullWhenNotSet()
        {
            var chk = new CHECKLIST();
            Assert.Null(chk.ASSET.HOST_NAME);
        }

        [Fact]
        public void Test_CHECKLIST_STIGS_VulnListIsEmptyWhenNotSet()
        {
            var chk = new CHECKLIST();
            Assert.Empty(chk.STIGS.iSTIG.VULN);
        }
    }
}
