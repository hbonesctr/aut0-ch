using Xunit;
using openrmf_msg_report.Models;

namespace tests.Models
{
    public class SettingsTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewSettings_IsNotNull()
        {
            var s = new Settings();
            Assert.NotNull(s);
        }

        [Fact]
        public void Test_Settings_ConnectionStringAndDatabase_AreSet()
        {
            var s = new Settings
            {
                ConnectionString = "mongodb://localhost:27017",
                Database = "openrmf"
            };

            Assert.Equal("mongodb://localhost:27017", s.ConnectionString);
            Assert.Equal("openrmf", s.Database);
        }

        [Theory]
        [InlineData("mongodb://localhost:27017", "openrmf")]
        [InlineData("mongodb://remotehost:27017", "reports")]
        public void Test_Settings_VariousConnections_AreValid(string conn, string db)
        {
            var s = new Settings { ConnectionString = conn, Database = db };
            Assert.NotNull(s.ConnectionString);
            Assert.NotNull(s.Database);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewSettings_ConnectionStringIsNullByDefault()
        {
            var s = new Settings();
            Assert.Null(s.ConnectionString);
        }

        [Fact]
        public void Test_NewSettings_DatabaseIsNullByDefault()
        {
            var s = new Settings();
            Assert.Null(s.Database);
        }

        [Fact]
        public void Test_Settings_ConnectionString_DoesNotEqualWrongValue()
        {
            var s = new Settings { ConnectionString = "mongodb://localhost:27017" };
            Assert.NotEqual("some-other-connection", s.ConnectionString);
        }
    }
}
