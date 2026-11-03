using Xunit;
using openrmf_msg_report.Models;

namespace tests.Models
{
    public class ASSETTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewASSET_IsNotNull()
        {
            var asset = new ASSET();
            Assert.NotNull(asset);
        }

        [Fact]
        public void Test_ASSETWithAllProperties_IsValid()
        {
            var asset = new ASSET
            {
                ROLE = "Member Server",
                ASSET_TYPE = "Computing",
                MARKING = "CUI",
                HOST_NAME = "WEBSERVER01",
                HOST_IP = "10.0.0.1",
                HOST_MAC = "AA:BB:CC:DD:EE:FF",
                HOST_FQDN = "webserver01.example.com",
                TECH_AREA = "Application",
                TARGET_KEY = "99999",
                WEB_OR_DATABASE = "false",
                WEB_DB_SITE = "",
                WEB_DB_INSTANCE = ""
            };

            Assert.Equal("Member Server", asset.ROLE);
            Assert.Equal("Computing", asset.ASSET_TYPE);
            Assert.Equal("CUI", asset.MARKING);
            Assert.Equal("WEBSERVER01", asset.HOST_NAME);
            Assert.Equal("10.0.0.1", asset.HOST_IP);
            Assert.Equal("AA:BB:CC:DD:EE:FF", asset.HOST_MAC);
            Assert.Equal("webserver01.example.com", asset.HOST_FQDN);
            Assert.Equal("Application", asset.TECH_AREA);
            Assert.Equal("99999", asset.TARGET_KEY);
            Assert.Equal("false", asset.WEB_OR_DATABASE);
        }

        [Fact]
        public void Test_ASSET_WebDatabaseProperties_SetCorrectly()
        {
            var asset = new ASSET
            {
                WEB_OR_DATABASE = "true",
                WEB_DB_SITE = "DefaultWebSite",
                WEB_DB_INSTANCE = "MyDatabase"
            };

            Assert.Equal("true", asset.WEB_OR_DATABASE);
            Assert.Equal("DefaultWebSite", asset.WEB_DB_SITE);
            Assert.Equal("MyDatabase", asset.WEB_DB_INSTANCE);
        }

        [Theory]
        [InlineData("Member Server")]
        [InlineData("Workstation")]
        [InlineData("None")]
        public void Test_ASSET_ROLE_AcceptsValidValues(string role)
        {
            var asset = new ASSET { ROLE = role };
            Assert.Equal(role, asset.ROLE);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewASSET_AllPropertiesAreNullByDefault()
        {
            var asset = new ASSET();
            Assert.Null(asset.ROLE);
            Assert.Null(asset.ASSET_TYPE);
            Assert.Null(asset.MARKING);
            Assert.Null(asset.HOST_NAME);
            Assert.Null(asset.HOST_IP);
            Assert.Null(asset.HOST_MAC);
            Assert.Null(asset.HOST_FQDN);
            Assert.Null(asset.TECH_AREA);
            Assert.Null(asset.TARGET_KEY);
            Assert.Null(asset.WEB_OR_DATABASE);
            Assert.Null(asset.WEB_DB_SITE);
            Assert.Null(asset.WEB_DB_INSTANCE);
        }

        [Fact]
        public void Test_ASSET_ROLE_DoesNotEqual_Wrong_Value()
        {
            var asset = new ASSET { ROLE = "Member Server" };
            Assert.NotEqual("Workstation", asset.ROLE);
        }
    }
}
