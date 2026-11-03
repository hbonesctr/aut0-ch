using Xunit;
using openrmf_msg_report.Classes;
using openrmf_msg_report.Models;

namespace tests.Classes
{
    public class ChecklistLoaderTests
    {
        // ── Minimal valid CKL XML used by multiple tests ───────────────────────
        private const string ValidCklXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<CHECKLIST>
  <ASSET>
    <ROLE>Member Server</ROLE>
    <ASSET_TYPE>Computing</ASSET_TYPE>
    <MARKING>CUI</MARKING>
    <HOST_NAME>TESTHOST</HOST_NAME>
    <HOST_IP>10.0.0.1</HOST_IP>
    <HOST_MAC>AA:BB:CC:DD:EE:FF</HOST_MAC>
    <HOST_FQDN>testhost.example.com</HOST_FQDN>
    <TECH_AREA></TECH_AREA>
    <TARGET_KEY>1234</TARGET_KEY>
    <WEB_OR_DATABASE>false</WEB_OR_DATABASE>
    <WEB_DB_SITE></WEB_DB_SITE>
    <WEB_DB_INSTANCE></WEB_DB_INSTANCE>
  </ASSET>
  <STIGS>
    <iSTIG>
      <STIG_INFO>
        <SI_DATA>
          <SID_NAME>version</SID_NAME>
          <SID_DATA>1</SID_DATA>
        </SI_DATA>
        <SI_DATA>
          <SID_NAME>title</SID_NAME>
          <SID_DATA>Windows 10 Security Technical Implementation Guide</SID_DATA>
        </SI_DATA>
        <SI_DATA>
          <SID_NAME>releaseinfo</SID_NAME>
          <SID_DATA>Release: 23 Benchmark Date: 26 Jan 2023</SID_DATA>
        </SI_DATA>
      </STIG_INFO>
      <VULN>
        <STIG_DATA>
          <VULN_ATTRIBUTE>Vuln_Num</VULN_ATTRIBUTE>
          <ATTRIBUTE_DATA>V-220697</ATTRIBUTE_DATA>
        </STIG_DATA>
        <STIG_DATA>
          <VULN_ATTRIBUTE>Severity</VULN_ATTRIBUTE>
          <ATTRIBUTE_DATA>high</ATTRIBUTE_DATA>
        </STIG_DATA>
        <STIG_DATA>
          <VULN_ATTRIBUTE>Check_Content</VULN_ATTRIBUTE>
          <ATTRIBUTE_DATA>Check this setting.</ATTRIBUTE_DATA>
        </STIG_DATA>
        <STATUS>NotAFinding</STATUS>
        <FINDING_DETAILS>No Finding</FINDING_DETAILS>
        <COMMENTS>Reviewed</COMMENTS>
        <SEVERITY_OVERRIDE></SEVERITY_OVERRIDE>
        <SEVERITY_JUSTIFICATION></SEVERITY_JUSTIFICATION>
      </VULN>
      <VULN>
        <STIG_DATA>
          <VULN_ATTRIBUTE>Vuln_Num</VULN_ATTRIBUTE>
          <ATTRIBUTE_DATA>V-220698</ATTRIBUTE_DATA>
        </STIG_DATA>
        <STIG_DATA>
          <VULN_ATTRIBUTE>Severity</VULN_ATTRIBUTE>
          <ATTRIBUTE_DATA>medium</ATTRIBUTE_DATA>
        </STIG_DATA>
        <STATUS>Open</STATUS>
        <FINDING_DETAILS>Finding found here.</FINDING_DETAILS>
        <COMMENTS></COMMENTS>
        <SEVERITY_OVERRIDE></SEVERITY_OVERRIDE>
        <SEVERITY_JUSTIFICATION></SEVERITY_JUSTIFICATION>
      </VULN>
    </iSTIG>
  </STIGS>
</CHECKLIST>";

        // ── LoadChecklist – Pass tests ─────────────────────────────────────────

        [Fact]
        public void Test_LoadChecklist_ReturnsNotNullChecklist()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.NotNull(checklist);
        }

        [Fact]
        public void Test_LoadChecklist_ASSETIsPopulated()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.NotNull(checklist.ASSET);
            Assert.Equal("TESTHOST", checklist.ASSET.HOST_NAME);
            Assert.Equal("Member Server", checklist.ASSET.ROLE);
            Assert.Equal("Computing", checklist.ASSET.ASSET_TYPE);
            Assert.Equal("CUI", checklist.ASSET.MARKING);
            Assert.Equal("10.0.0.1", checklist.ASSET.HOST_IP);
            Assert.Equal("AA:BB:CC:DD:EE:FF", checklist.ASSET.HOST_MAC);
            Assert.Equal("testhost.example.com", checklist.ASSET.HOST_FQDN);
            Assert.Equal("1234", checklist.ASSET.TARGET_KEY);
            Assert.Equal("false", checklist.ASSET.WEB_OR_DATABASE);
        }

        [Fact]
        public void Test_LoadChecklist_STIG_INFOIsPopulated()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.NotNull(checklist.STIGS.iSTIG.STIG_INFO);
            Assert.Equal(3, checklist.STIGS.iSTIG.STIG_INFO.SI_DATA.Count);
        }

        [Fact]
        public void Test_LoadChecklist_STIG_INFO_VersionIsCorrect()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            var version = checklist.STIGS.iSTIG.STIG_INFO.SI_DATA.Find(x => x.SID_NAME == "version");
            Assert.NotNull(version);
            Assert.Equal("1", version.SID_DATA);
        }

        [Fact]
        public void Test_LoadChecklist_STIG_INFO_TitleIsCorrect()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            var title = checklist.STIGS.iSTIG.STIG_INFO.SI_DATA.Find(x => x.SID_NAME == "title");
            Assert.NotNull(title);
            Assert.Equal("Windows 10 Security Technical Implementation Guide", title.SID_DATA);
        }

        [Fact]
        public void Test_LoadChecklist_VULNListIsPopulated()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.Equal(2, checklist.STIGS.iSTIG.VULN.Count);
        }

        [Fact]
        public void Test_LoadChecklist_FirstVULN_StatusIsNotAFinding()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.Equal("NotAFinding", checklist.STIGS.iSTIG.VULN[0].STATUS);
        }

        [Fact]
        public void Test_LoadChecklist_SecondVULN_StatusIsOpen()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.Equal("Open", checklist.STIGS.iSTIG.VULN[1].STATUS);
        }

        [Fact]
        public void Test_LoadChecklist_FirstVULN_HasSTIG_DATAEntries()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            var vuln = checklist.STIGS.iSTIG.VULN[0];
            Assert.NotEmpty(vuln.STIG_DATA);

            var vulnNum = vuln.STIG_DATA.Find(x => x.VULN_ATTRIBUTE == "Vuln_Num");
            Assert.NotNull(vulnNum);
            Assert.Equal("V-220697", vulnNum.ATTRIBUTE_DATA);
        }

        [Fact]
        public void Test_LoadChecklist_FirstVULN_FindingDetailsSet()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.Equal("No Finding", checklist.STIGS.iSTIG.VULN[0].FINDING_DETAILS);
        }

        [Fact]
        public void Test_LoadChecklist_FirstVULN_CommentsSet()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.Equal("Reviewed", checklist.STIGS.iSTIG.VULN[0].COMMENTS);
        }

        // ── SanitizeChecklistType – Pass tests ────────────────────────────────

        [Fact]
        public void Test_SanitizeChecklistType_ReplacesSecurityTechnicalImplementationGuide()
        {
            var result = ChecklistLoader.SanitizeChecklistType(
                "Windows 10 Security Technical Implementation Guide");
            Assert.Contains("STIG", result);
            Assert.DoesNotContain("Security Technical Implementation Guide", result);
        }

        [Fact]
        public void Test_SanitizeChecklistType_ReplacesWindowsServer()
        {
            var result = ChecklistLoader.SanitizeChecklistType("Windows Server 2019 STIG");
            Assert.Contains("WIN SVR", result);
            Assert.DoesNotContain("Windows Server", result);
        }

        [Fact]
        public void Test_SanitizeChecklistType_ReplacesMSWindows()
        {
            var result = ChecklistLoader.SanitizeChecklistType("MS Windows 10 STIG");
            Assert.DoesNotContain("MS Windows", result);
        }

        [Fact]
        public void Test_SanitizeChecklistType_ReplacesWindows10()
        {
            var result = ChecklistLoader.SanitizeChecklistType("Windows 10 STIG");
            Assert.Contains("WIN 10", result);
        }

        [Theory]
        [InlineData("Windows 10 Security Technical Implementation Guide", "WIN 10")]
        [InlineData("Windows 11 Security Technical Implementation Guide", "WIN 11")]
        [InlineData("Application Security and Development STIG", "ASD")]
        public void Test_SanitizeChecklistType_KnownPatterns_AreReplaced(string input, string expectedFragment)
        {
            var result = ChecklistLoader.SanitizeChecklistType(input);
            Assert.Contains(expectedFragment, result);
        }

        // ── SanitizeChecklistRelease – Pass tests ─────────────────────────────

        [Fact]
        public void Test_SanitizeChecklistRelease_ReplacesReleasePrefix()
        {
            var result = ChecklistLoader.SanitizeChecklistRelease("Release: 23 Benchmark Date: 26 Jan 2023");
            Assert.StartsWith("R23", result);
            Assert.DoesNotContain("Release: ", result);
        }

        [Fact]
        public void Test_SanitizeChecklistRelease_ReplacesBenchmarkDate()
        {
            var result = ChecklistLoader.SanitizeChecklistRelease("Release: 5 Benchmark Date: 01 Jan 2022");
            Assert.Contains("dated", result);
            Assert.DoesNotContain("Benchmark Date:", result);
        }

        [Theory]
        [InlineData("Release: 1 Benchmark Date: 01 Jan 2020", "R1")]
        [InlineData("Release: 10 Benchmark Date: 15 Mar 2023", "R10")]
        public void Test_SanitizeChecklistRelease_VariousReleases_AreFormatted(string input, string expectedFragment)
        {
            var result = ChecklistLoader.SanitizeChecklistRelease(input);
            Assert.StartsWith(expectedFragment, result);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_LoadChecklist_WithTabsInXml_HandledCorrectly()
        {
            // ChecklistLoader replaces tabs; verify it doesn't throw
            string xml = ValidCklXml.Replace("  ", "\t");
            var checklist = ChecklistLoader.LoadChecklist(xml);
            Assert.NotNull(checklist);
        }

        [Fact]
        public void Test_LoadChecklist_ASSET_HOST_NAME_DoesNotMatchWrongValue()
        {
            var checklist = ChecklistLoader.LoadChecklist(ValidCklXml);
            Assert.NotEqual("WRONGHOST", checklist.ASSET.HOST_NAME);
        }

        [Fact]
        public void Test_SanitizeChecklistType_EmptyString_ReturnsEmpty()
        {
            var result = ChecklistLoader.SanitizeChecklistType(string.Empty);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Test_SanitizeChecklistRelease_StringWithNoKeywords_ReturnsUnchanged()
        {
            const string input = "No keywords here";
            var result = ChecklistLoader.SanitizeChecklistRelease(input);
            Assert.Equal(input, result);
        }
    }
}
