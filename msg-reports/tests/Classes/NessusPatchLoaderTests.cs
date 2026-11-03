using Xunit;
using openrmf_msg_report.Classes;
using openrmf_msg_report.Models;
using System.Collections.Generic;

namespace tests.Classes
{
    public class NessusPatchLoaderTests
    {
        // ── Minimal valid Nessus XML used by multiple tests ────────────────────
        private const string ValidNessusXml = @"<?xml version=""1.0"" ?>
<NessusClientData_v2>
  <Report name=""MyTestReport"">
    <ReportHost name=""192.168.1.100"">
      <HostProperties>
        <tag name=""netbios-name"">FILESERVER01</tag>
        <tag name=""hostname"">fileserver01</tag>
        <tag name=""operating-system"">Windows Server 2019</tag>
        <tag name=""system-type"">general-purpose</tag>
        <tag name=""Credentialed_Scan"">true</tag>
        <tag name=""host-rdns"">192.168.1.100</tag>
      </HostProperties>
      <ReportItem port=""0"" svc_name=""general"" protocol=""tcp"" severity=""2""
                  pluginID=""12345"" pluginName=""Test Medium Plugin"" pluginFamily=""General"">
        <description>Test medium description</description>
        <plugin_publication_date>2022/01/01</plugin_publication_date>
        <plugin_type>local</plugin_type>
        <risk_factor>Medium</risk_factor>
        <synopsis>Test synopsis</synopsis>
      </ReportItem>
      <ReportItem port=""0"" svc_name=""general"" protocol=""tcp"" severity=""4""
                  pluginID=""99999"" pluginName=""Critical Plugin"" pluginFamily=""Windows"">
        <description>Critical issue description</description>
        <plugin_publication_date>2023/06/15</plugin_publication_date>
        <plugin_type>local</plugin_type>
        <risk_factor>Critical</risk_factor>
        <synopsis>Critical synopsis</synopsis>
      </ReportItem>
    </ReportHost>
  </Report>
</NessusClientData_v2>";

        // ── LoadPatchData – Pass tests ─────────────────────────────────────────

        [Fact]
        public void Test_LoadPatchData_ReturnsNonNullList()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            Assert.NotNull(result);
        }

        [Fact]
        public void Test_LoadPatchData_ReturnsCorrectItemCount()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Test_LoadPatchData_ReportNameIsPopulated()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            foreach (var item in result)
                Assert.Equal("MyTestReport", item.reportName);
        }

        [Fact]
        public void Test_LoadPatchData_FirstItem_SeverityIsMedium()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            var mediumItem = result.Find(x => x.pluginId == "12345");
            Assert.NotNull(mediumItem);
            Assert.Equal(2, mediumItem.severity);
            Assert.Equal("Medium", mediumItem.severityName);
        }

        [Fact]
        public void Test_LoadPatchData_SecondItem_SeverityIsCritical()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            var criticalItem = result.Find(x => x.pluginId == "99999");
            Assert.NotNull(criticalItem);
            Assert.Equal(4, criticalItem.severity);
            Assert.Equal("Critical", criticalItem.severityName);
        }

        [Fact]
        public void Test_LoadPatchData_FirstItem_PluginNameIsCorrect()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            var item = result.Find(x => x.pluginId == "12345");
            Assert.Equal("Test Medium Plugin", item.pluginName);
        }

        [Fact]
        public void Test_LoadPatchData_FirstItem_FamilyIsCorrect()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            var item = result.Find(x => x.pluginId == "12345");
            Assert.Equal("General", item.family);
        }

        [Fact]
        public void Test_LoadPatchData_FirstItem_DescriptionIsPopulated()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            var item = result.Find(x => x.pluginId == "12345");
            Assert.Equal("Test medium description", item.description);
        }

        [Fact]
        public void Test_LoadPatchData_FirstItem_SynopsisIsPopulated()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            var item = result.Find(x => x.pluginId == "12345");
            Assert.Equal("Test synopsis", item.synopsis);
        }

        [Fact]
        public void Test_LoadPatchData_Items_OperatingSystemIsPopulated()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            foreach (var item in result)
                Assert.Equal("Windows Server 2019", item.operatingSystem);
        }

        [Fact]
        public void Test_LoadPatchData_Items_SystemTypeIsPopulated()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            foreach (var item in result)
                Assert.Equal("general-purpose", item.systemType);
        }

        [Fact]
        public void Test_LoadPatchData_Items_CredentialedIsTrue()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            foreach (var item in result)
                Assert.True(item.credentialed);
        }

        // ── SanitizeHostname – Pass tests ──────────────────────────────────────

        [Fact]
        public void Test_SanitizeHostname_PlainHostname_ReturnsUnchanged()
        {
            var result = NessusPatchLoader.SanitizeHostname("myserver");
            Assert.Equal("myserver", result);
        }

        [Fact]
        public void Test_SanitizeHostname_ValidIP_MasksFirstTwoOctets()
        {
            var result = NessusPatchLoader.SanitizeHostname("192.168.10.55");
            Assert.Equal("xxx.xxx.10.55", result);
        }

        [Fact]
        public void Test_SanitizeHostname_AnotherValidIP_MasksCorrectly()
        {
            var result = NessusPatchLoader.SanitizeHostname("10.0.5.1");
            Assert.Equal("xxx.xxx.5.1", result);
        }

        [Fact]
        public void Test_SanitizeHostname_FQDN_ReturnsUnchanged()
        {
            var result = NessusPatchLoader.SanitizeHostname("server.example.com");
            Assert.Equal("server.example.com", result);
        }

        [Theory]
        [InlineData("10.0.0.1", "xxx.xxx.0.1")]
        [InlineData("172.16.100.200", "xxx.xxx.100.200")]
        [InlineData("192.168.255.255", "xxx.xxx.255.255")]
        public void Test_SanitizeHostname_IPAddresses_MaskFirstTwoOctets(string ip, string expected)
        {
            var result = NessusPatchLoader.SanitizeHostname(ip);
            Assert.Equal(expected, result);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_LoadPatchData_EmptyReport_ReturnsEmptyList()
        {
            const string emptyReport = @"<?xml version=""1.0"" ?>
<NessusClientData_v2>
  <Report name=""EmptyReport"">
  </Report>
</NessusClientData_v2>";

            var result = NessusPatchLoader.LoadPatchData(emptyReport);
            Assert.Empty(result);
        }

        [Fact]
        public void Test_LoadPatchData_FirstItem_SeverityDoesNotMatchWrongName()
        {
            var result = NessusPatchLoader.LoadPatchData(ValidNessusXml);
            var item = result.Find(x => x.pluginId == "12345");
            Assert.NotEqual("Critical", item.severityName);
        }

        [Fact]
        public void Test_SanitizeHostname_EmptyString_ReturnsEmpty()
        {
            var result = NessusPatchLoader.SanitizeHostname(string.Empty);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Test_SanitizeHostname_PlainName_DoesNotContainXxx()
        {
            var result = NessusPatchLoader.SanitizeHostname("myserver");
            Assert.DoesNotContain("xxx", result);
        }
    }
}
