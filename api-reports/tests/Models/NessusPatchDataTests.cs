using Xunit;
using openrmf_report_api.Models;
using System;

namespace tests.Models
{
    public class NessusPatchDataTests
    {
        // ---- Pass Tests ----

        [Fact]
        public void Test_NewNessusPatchDataIsValid()
        {
            NessusPatchData data = new NessusPatchData();
            Assert.NotNull(data);
        }

        [Fact]
        public void Test_NessusPatchDataWithDataIsValid()
        {
            NessusPatchData data = new NessusPatchData();
            data.created = DateTime.Now;
            data.systemGroupId = "875678654gghjghjkgu658";
            data.hostname = "myHost";
            data.reportName = "My Report Here";
            data.updatedOn = DateTime.Now;
            data.operatingSystem = "Windows";
            data.systemType = "My System Type";
            data.ipAddress = "10.10.10.111";
            data.credentialed = true;
            data.pluginId = "9689658";
            data.pluginName = "My Plugin";
            data.family = "My Family";
            data.severity = 4;
            data.hostTotal = 2;
            data.total = 3;
            data.description = "This is my description";
            data.publicationDate = "March 31, 2020";
            data.pluginType = "My Plugin Type";
            data.riskFactor = "My Risk";
            data.synopsis = "My synopsis";
            data.scanVersion = "6.11.1";

            Assert.NotNull(data);
            Assert.NotEmpty(data.created.ToShortDateString());
            Assert.NotEmpty(data.systemGroupId);
            Assert.NotEmpty(data.hostname);
            Assert.NotEmpty(data.reportName);
            Assert.NotEmpty(data.operatingSystem);
            Assert.NotEmpty(data.systemType);
            Assert.NotEmpty(data.ipAddress);
            Assert.True(data.credentialed);
            Assert.NotEmpty(data.pluginId);
            Assert.NotEmpty(data.pluginName);
            Assert.NotEmpty(data.family);
            Assert.Equal(4, data.severity);
            Assert.Equal(2, data.hostTotal);
            Assert.Equal(3, data.total);
            Assert.NotEmpty(data.description);
            Assert.NotEmpty(data.publicationDate);
            Assert.NotEmpty(data.pluginType);
            Assert.NotEmpty(data.riskFactor);
            Assert.NotEmpty(data.synopsis);
            Assert.Equal("Critical", data.severityName);
            Assert.True(data.updatedOn.HasValue);
            Assert.NotEmpty(data.updatedOn.Value.ToShortDateString());
        }

        [Theory]
        [InlineData(4, "Critical")]
        [InlineData(3, "High")]
        [InlineData(2, "Medium")]
        [InlineData(1, "Low")]
        [InlineData(0, "Informational")]
        public void Test_SeverityNameReturnsCorrectLabel(int severityValue, string expectedLabel)
        {
            NessusPatchData data = new NessusPatchData { severity = severityValue };
            Assert.Equal(expectedLabel, data.severityName);
        }

        [Fact]
        public void Test_PluginIdSortPadsShortId()
        {
            NessusPatchData data = new NessusPatchData { pluginId = "123" };
            Assert.Equal("0123", data.pluginIdSort);
        }

        [Fact]
        public void Test_PluginIdSortDoesNotPadLongId()
        {
            NessusPatchData data = new NessusPatchData { pluginId = "123456" };
            Assert.Equal("123456", data.pluginIdSort);
        }

        [Fact]
        public void Test_UpdatedOnNullableIsNullByDefault()
        {
            NessusPatchData data = new NessusPatchData();
            Assert.False(data.updatedOn.HasValue);
        }

        [Fact]
        public void Test_CredentialedDefaultIsFalse()
        {
            NessusPatchData data = new NessusPatchData();
            Assert.False(data.credentialed);
        }

        // ---- Fail / Negative Tests ----

        [Fact]
        public void Test_SeverityNameIsNotCriticalForLowSeverity()
        {
            NessusPatchData data = new NessusPatchData { severity = 1 };
            Assert.NotEqual("Critical", data.severityName);
        }

        [Fact]
        public void Test_SystemGroupIdIsNullByDefault()
        {
            NessusPatchData data = new NessusPatchData();
            Assert.Null(data.systemGroupId);
        }

        [Fact]
        public void Test_HostnameIsNullByDefault()
        {
            NessusPatchData data = new NessusPatchData();
            Assert.Null(data.hostname);
        }
    }
}
