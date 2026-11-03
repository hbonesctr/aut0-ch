using Xunit;
using openrmf_report_api.Models;

namespace tests.Models
{
    public class SettingsTests
    {
        // ---- Pass Tests ----

        [Fact]
        public void Test_NewSettingsIsValid()
        {
            Settings settings = new Settings();
            Assert.NotNull(settings);
        }

        [Fact]
        public void Test_SettingsConnectionStringCanBeSet()
        {
            Settings settings = new Settings();
            settings.ConnectionString = "mongodb://localhost:27017";
            Assert.Equal("mongodb://localhost:27017", settings.ConnectionString);
        }

        [Fact]
        public void Test_SettingsDatabaseCanBeSet()
        {
            Settings settings = new Settings();
            settings.Database = "openrmf";
            Assert.Equal("openrmf", settings.Database);
        }

        [Fact]
        public void Test_SettingsWithFullDataIsValid()
        {
            Settings settings = new Settings();
            settings.ConnectionString = "myConnection";
            settings.Database = "user=x; database=x; password=x;";

            Assert.NotNull(settings);
            Assert.NotEmpty(settings.ConnectionString);
            Assert.NotEmpty(settings.Database);
        }

        // ---- Fail / Negative Tests ----

        [Fact]
        public void Test_ConnectionStringIsNullByDefault()
        {
            Settings settings = new Settings();
            Assert.Null(settings.ConnectionString);
        }

        [Fact]
        public void Test_DatabaseIsNullByDefault()
        {
            Settings settings = new Settings();
            Assert.Null(settings.Database);
        }

        [Fact]
        public void Test_ConnectionStringIsNotDatabase()
        {
            Settings settings = new Settings();
            settings.ConnectionString = "connA";
            settings.Database = "dbB";
            Assert.NotEqual(settings.ConnectionString, settings.Database);
        }
    }
}
