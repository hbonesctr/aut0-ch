using Xunit;
using openrmf_msg_report.Models;

namespace tests.Models
{
    public class STIG_INFOTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewSTIG_INFO_IsNotNull()
        {
            var info = new STIG_INFO();
            Assert.NotNull(info);
        }

        [Fact]
        public void Test_NewSTIG_INFO_SI_DATAIsInitializedAndEmpty()
        {
            var info = new STIG_INFO();
            Assert.NotNull(info.SI_DATA);
            Assert.Empty(info.SI_DATA);
        }

        [Fact]
        public void Test_STIG_INFO_SI_DATA_CanAddItems()
        {
            var info = new STIG_INFO();
            info.SI_DATA.Add(new SI_DATA { SID_NAME = "version", SID_DATA = "1" });
            info.SI_DATA.Add(new SI_DATA { SID_NAME = "title", SID_DATA = "Windows 10 STIG" });

            Assert.Equal(2, info.SI_DATA.Count);
        }

        [Fact]
        public void Test_STIG_INFO_SI_DATA_CanFindByName()
        {
            var info = new STIG_INFO();
            info.SI_DATA.Add(new SI_DATA { SID_NAME = "version", SID_DATA = "3" });
            info.SI_DATA.Add(new SI_DATA { SID_NAME = "releaseinfo", SID_DATA = "Release: 5" });

            var versionEntry = info.SI_DATA.Find(x => x.SID_NAME == "version");
            Assert.NotNull(versionEntry);
            Assert.Equal("3", versionEntry.SID_DATA);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewSTIG_INFO_SI_DATACountIsZeroWhenNotPopulated()
        {
            var info = new STIG_INFO();
            Assert.Equal(0, info.SI_DATA.Count);
        }

        [Fact]
        public void Test_STIG_INFO_SI_DATA_FindMissingKeyReturnsNull()
        {
            var info = new STIG_INFO();
            info.SI_DATA.Add(new SI_DATA { SID_NAME = "version", SID_DATA = "1" });

            var missing = info.SI_DATA.Find(x => x.SID_NAME == "nonexistent");
            Assert.Null(missing);
        }
    }
}
