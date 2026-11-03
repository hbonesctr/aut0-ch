using Xunit;
using openrmf_msg_report.Models;
using System;

namespace tests.Models
{
    public class NessusPatchDataTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewNessusPatchData_IsNotNull()
        {
            var data = new NessusPatchData();
            Assert.NotNull(data);
        }

        [Fact]
        public void Test_NessusPatchData_AllProperties_AreSet()
        {
            var data = new NessusPatchData
            {
                created = DateTime.Now,
                systemGroupId = "875678654gghjghjkgu658",
                hostname = "myHost",
                reportName = "My Report Here",
                updatedOn = DateTime.Now,
                operatingSystem = "Windows",
                systemType = "My System Type",
                ipAddress = "10.10.10.111",
                credentialed = true,
                pluginId = "9689658",
                pluginName = "My Plugin",
                family = "My Family",
                severity = 4,
                hostTotal = 2,
                total = 3,
                description = "This is my description",
                publicationDate = "March 31, 2020",
                pluginType = "My Plugin Type",
                riskFactor = "My Risk",
                synopsis = "My synopsis",
                scanVersion = "8.15.0"
            };

            Assert.Equal("875678654gghjghjkgu658", data.systemGroupId);
            Assert.Equal("myHost", data.hostname);
            Assert.Equal("My Report Here", data.reportName);
            Assert.Equal("Windows", data.operatingSystem);
            Assert.Equal("My System Type", data.systemType);
            Assert.Equal("10.10.10.111", data.ipAddress);
            Assert.True(data.credentialed);
            Assert.Equal("9689658", data.pluginId);
            Assert.Equal("My Plugin", data.pluginName);
            Assert.Equal("My Family", data.family);
            Assert.Equal(4, data.severity);
            Assert.Equal(2, data.hostTotal);
            Assert.Equal(3, data.total);
            Assert.Equal("This is my description", data.description);
            Assert.True(data.updatedOn.HasValue);
        }

        [Theory]
        [InlineData(4, "Critical")]
        [InlineData(3, "High")]
        [InlineData(2, "Medium")]
        [InlineData(1, "Low")]
        [InlineData(0, "Informational")]
        public void Test_NessusPatchData_SeverityName_MapsCorrectly(int severity, string expectedName)
        {
            var data = new NessusPatchData { severity = severity };
            Assert.Equal(expectedName, data.severityName);
        }

        [Fact]
        public void Test_NessusPatchData_PluginIdSort_PadsShortId()
        {
            var data = new NessusPatchData { pluginId = "1234" };
            Assert.Equal("01234", data.pluginIdSort);
        }

        [Fact]
        public void Test_NessusPatchData_PluginIdSort_DoesNotPadLongId()
        {
            var data = new NessusPatchData { pluginId = "123456" };
            Assert.Equal("123456", data.pluginIdSort);
        }

        [Fact]
        public void Test_NessusPatchData_InternalIdString_IsString()
        {
            var data = new NessusPatchData();
            Assert.NotNull(data.InternalIdString);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewNessusPatchData_HostnameIsNullByDefault()
        {
            var data = new NessusPatchData();
            Assert.Null(data.hostname);
        }

        [Fact]
        public void Test_NewNessusPatchData_CredentialedIsFalseByDefault()
        {
            var data = new NessusPatchData();
            Assert.False(data.credentialed);
        }

        [Fact]
        public void Test_NessusPatchData_SeverityName_DoesNotMatchWrongSeverity()
        {
            var data = new NessusPatchData { severity = 4 };
            Assert.NotEqual("Low", data.severityName);
        }

        [Fact]
        public void Test_NessusPatchData_UpdatedOn_IsNullByDefault()
        {
            var data = new NessusPatchData();
            Assert.False(data.updatedOn.HasValue);
        }
    }
}
