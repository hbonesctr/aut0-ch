using System;
using System.Linq;
using openrmf_read_api.Classes;
using openrmf_read_api.Models;
using Xunit;

namespace tests.Classes;

/// <summary>AUT0-CH: STIG Viewer 3 CKLB import.</summary>
public class CklbConverterTests
{
    private const string SingleStig = @"{
  ""title"": ""web01 checklist"",
  ""id"": ""11111111-1111-1111-1111-111111111111"",
  ""cklb_version"": ""1.0"",
  ""target_data"": {
    ""target_type"": ""Computing"",
    ""host_name"": ""web01"",
    ""ip_address"": ""10.1.2.3"",
    ""mac_address"": ""00:11:22:33:44:55"",
    ""fqdn"": ""web01.example.mil"",
    ""comments"": ""DMZ & internal"",
    ""role"": ""Member Server"",
    ""is_web_database"": false,
    ""technology_area"": ""Web Server"",
    ""web_db_site"": """",
    ""web_db_instance"": """"
  },
  ""stigs"": [
    {
      ""stig_name"": ""Microsoft Windows Server 2022 Security Technical Implementation Guide"",
      ""display_name"": ""Windows Server 2022"",
      ""stig_id"": ""MS_Windows_Server_2022_STIG"",
      ""release_info"": ""Release: 5 Benchmark Date: 24 Jul 2025"",
      ""version"": ""2"",
      ""uuid"": ""aaaaaaaa-0000-0000-0000-000000000001"",
      ""reference_identifier"": ""5470"",
      ""rules"": [
        {
          ""group_id"": ""V-254238"",
          ""rule_id"": ""SV-254238"",
          ""rule_id_src"": ""SV-254238r958472_rule"",
          ""rule_version"": ""WN22-00-000010"",
          ""rule_title"": ""Users must be <limited> & audited"",
          ""group_title"": ""SRG-OS-000080-GPOS-00048"",
          ""severity"": ""high"",
          ""weight"": ""10.0"",
          ""discussion"": ""Discussion text"",
          ""check_content"": ""Check text"",
          ""fix_text"": ""Fix text"",
          ""target_key"": ""5470"",
          ""stig_uuid"": ""aaaaaaaa-0000-0000-0000-000000000001"",
          ""legacy_ids"": [""V-93369"", ""SV-103455""],
          ""ccis"": [""CCI-000213"", ""CCI-002235""],
          ""check_content_ref"": { ""href"": ""Windows_Server_2022.xml"", ""name"": ""M"" },
          ""status"": ""open"",
          ""finding_details"": ""Found 3 accounts"",
          ""comments"": ""Ticket 42"",
          ""overrides"": { ""severity"": { ""severity"": ""medium"", ""reason"": ""Mitigated by firewall"" } }
        },
        {
          ""group_id"": ""V-254239"",
          ""rule_id_src"": ""SV-254239r958474_rule"",
          ""rule_version"": ""WN22-00-000020"",
          ""rule_title"": ""Second rule"",
          ""severity"": ""medium"",
          ""ccis"": [""CCI-000366""],
          ""status"": ""not_a_finding""
        },
        {
          ""group_id"": ""V-254240"",
          ""rule_version"": ""WN22-00-000030"",
          ""rule_title"": ""Third rule"",
          ""severity"": ""low"",
          ""status"": ""not_applicable""
        },
        {
          ""group_id"": ""V-254241"",
          ""rule_version"": ""WN22-00-000040"",
          ""rule_title"": ""Fourth rule"",
          ""severity"": ""medium"",
          ""status"": ""not_reviewed""
        }
      ]
    }
  ]
}";

    private const string MultiStig = @"{
  ""target_data"": { ""host_name"": ""db01"", ""is_web_database"": true, ""web_db_site"": ""site1"", ""web_db_instance"": ""inst1"" },
  ""stigs"": [
    { ""stig_name"": ""Oracle Database 19c Security Technical Implementation Guide"", ""version"": ""1"", ""release_info"": ""Release: 3"",
      ""rules"": [ { ""group_id"": ""V-1"", ""rule_version"": ""O19C-00-000100"", ""severity"": ""medium"", ""status"": ""open"" } ] },
    { ""stig_name"": ""Red Hat Enterprise Linux 9 Security Technical Implementation Guide"", ""version"": ""2"", ""release_info"": ""Release: 4"",
      ""rules"": [ { ""group_id"": ""V-2"", ""rule_version"": ""RHEL-09-000001"", ""severity"": ""high"", ""status"": ""not_a_finding"" },
                   { ""group_id"": ""V-3"", ""rule_version"": ""RHEL-09-000002"", ""severity"": ""low"", ""status"": ""open"" } ] },
    { ""stig_name"": ""Empty STIG"", ""rules"": [] }
  ]
}";

    private static CHECKLIST LoadFirst(string cklb) =>
        ChecklistLoader.LoadChecklist(CklbConverter.ToCklList(cklb).First());

    private static string Attr(VULN v, string name) =>
        v.STIG_DATA.Where(d => d.VULN_ATTRIBUTE == name).Select(d => d.ATTRIBUTE_DATA).FirstOrDefault();

    [Fact]
    public void SingleStig_ProducesOneChecklist()
    {
        Assert.Single(CklbConverter.ToCklList(SingleStig));
    }

    [Fact]
    public void Asset_IsMappedFromTargetData()
    {
        var ckl = LoadFirst(SingleStig);
        Assert.Equal("web01", ckl.ASSET.HOST_NAME);
        Assert.Equal("10.1.2.3", ckl.ASSET.HOST_IP);
        Assert.Equal("00:11:22:33:44:55", ckl.ASSET.HOST_MAC);
        Assert.Equal("web01.example.mil", ckl.ASSET.HOST_FQDN);
        Assert.Equal("Member Server", ckl.ASSET.ROLE);
        Assert.Equal("Computing", ckl.ASSET.ASSET_TYPE);
        Assert.Equal("Web Server", ckl.ASSET.TECH_AREA);
        Assert.Equal("5470", ckl.ASSET.TARGET_KEY);
        Assert.Equal("false", ckl.ASSET.WEB_OR_DATABASE);
    }

    [Fact]
    public void StigInfo_HasTitleVersionAndRelease()
    {
        var info = LoadFirst(SingleStig).STIGS.iSTIG.STIG_INFO.SI_DATA;
        string Si(string n) => info.Where(s => s.SID_NAME == n).Select(s => s.SID_DATA).FirstOrDefault();
        Assert.Equal("Microsoft Windows Server 2022 Security Technical Implementation Guide", Si("title"));
        Assert.Equal("2", Si("version"));
        Assert.Equal("Release: 5 Benchmark Date: 24 Jul 2025", Si("releaseinfo"));
        Assert.Equal("MS_Windows_Server_2022_STIG", Si("stigid"));
    }

    [Fact]
    public void Statuses_AreMappedToCklValues()
    {
        var vulns = LoadFirst(SingleStig).STIGS.iSTIG.VULN;
        Assert.Equal(4, vulns.Count);
        Assert.Equal("Open", vulns[0].STATUS);
        Assert.Equal("NotAFinding", vulns[1].STATUS);
        Assert.Equal("Not_Applicable", vulns[2].STATUS);
        Assert.Equal("Not_Reviewed", vulns[3].STATUS);
    }

    [Fact]
    public void RuleFields_AreMapped_AndSpecialCharactersSurvive()
    {
        var v = LoadFirst(SingleStig).STIGS.iSTIG.VULN[0];
        Assert.Equal("V-254238", Attr(v, "Vuln_Num"));
        Assert.Equal("SV-254238r958472_rule", Attr(v, "Rule_ID"));
        Assert.Equal("WN22-00-000010", Attr(v, "Rule_Ver"));
        Assert.Equal("Users must be <limited> & audited", Attr(v, "Rule_Title"));
        Assert.Equal("high", Attr(v, "Severity"));
        Assert.Equal("M", Attr(v, "Check_Content_Ref"));
        Assert.Equal("Microsoft Windows Server 2022 Security Technical Implementation Guide :: Version 2, Release: 5 Benchmark Date: 24 Jul 2025", Attr(v, "STIGRef"));
        Assert.Equal("Found 3 accounts", v.FINDING_DETAILS);
        Assert.Equal("Ticket 42", v.COMMENTS);
    }

    [Fact]
    public void CcisAndLegacyIds_AreRepeatedStigDataEntries()
    {
        var v = LoadFirst(SingleStig).STIGS.iSTIG.VULN[0];
        Assert.Equal(new[] { "CCI-000213", "CCI-002235" },
            v.STIG_DATA.Where(d => d.VULN_ATTRIBUTE == "CCI_REF").Select(d => d.ATTRIBUTE_DATA).ToArray());
        Assert.Equal(new[] { "V-93369", "SV-103455" },
            v.STIG_DATA.Where(d => d.VULN_ATTRIBUTE == "LEGACY_ID").Select(d => d.ATTRIBUTE_DATA).ToArray());
    }

    [Fact]
    public void SeverityOverride_IsCarriedOver()
    {
        var v = LoadFirst(SingleStig).STIGS.iSTIG.VULN[0];
        Assert.Equal("medium", v.SEVERITY_OVERRIDE);
        Assert.Equal("Mitigated by firewall", v.SEVERITY_JUSTIFICATION);
    }

    [Fact]
    public void FlatSeverityOverride_IsAlsoAccepted()
    {
        string json = SingleStig.Replace(
            @"""overrides"": { ""severity"": { ""severity"": ""medium"", ""reason"": ""Mitigated by firewall"" } }",
            @"""overrides"": { ""severity"": ""LOW"", ""reason"": ""Flat form"" }");
        var v = LoadFirst(json).STIGS.iSTIG.VULN[0];
        Assert.Equal("low", v.SEVERITY_OVERRIDE);
        Assert.Equal("Flat form", v.SEVERITY_JUSTIFICATION);
    }

    [Fact]
    public void MultiStig_ProducesOneChecklistPerStigWithRules()
    {
        var list = CklbConverter.ToCklList(MultiStig);
        Assert.Equal(2, list.Count); // the empty STIG is skipped
        var rhel = ChecklistLoader.LoadChecklist(list[1]);
        Assert.Equal(2, rhel.STIGS.iSTIG.VULN.Count);
        Assert.Equal("db01", rhel.ASSET.HOST_NAME);
        Assert.Equal("true", rhel.ASSET.WEB_OR_DATABASE);
        Assert.Equal("site1", rhel.ASSET.WEB_DB_SITE);
        Assert.Equal("inst1", rhel.ASSET.WEB_DB_INSTANCE);
    }

    [Fact]
    public void ConvertedChecklists_GoThroughTheExistingUploadSplitter()
    {
        // the upload controller feeds each converted CKL through LoadMultipleChecklistTypes
        var results = CklbConverter.ToCklList(MultiStig)
            .SelectMany(ChecklistLoader.LoadMultipleChecklistTypes).ToList();
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.False(string.IsNullOrEmpty(r.rawChecklist)));
        var oracle = ChecklistLoader.LoadChecklist(results[0].rawChecklist);
        Assert.Single(oracle.STIGS.iSTIG.VULN);
        Assert.Equal("Open", oracle.STIGS.iSTIG.VULN[0].STATUS);
    }

    [Fact]
    public void FullUploadPath_PreservesSpecialCharactersAndOverrides()
    {
        var record = Assert.Single(CklbConverter.ToCklList(SingleStig)
            .SelectMany(ChecklistLoader.LoadMultipleChecklistTypes));
        var ckl = ChecklistLoader.LoadChecklist(record.rawChecklist);
        var v = ckl.STIGS.iSTIG.VULN[0];
        Assert.Equal("Users must be <limited> & audited", Attr(v, "Rule_Title"));
        Assert.Equal("medium", v.SEVERITY_OVERRIDE);
        Assert.Equal("web01", ckl.ASSET.HOST_NAME);
        Assert.Equal(4, ckl.STIGS.iSTIG.VULN.Count);
    }

    [Fact]
    public void ControlCharacters_AreStripped_NotFatal()
    {
        string json = SingleStig.Replace("Found 3 accounts", "Found\\u0001 3\\u000B accounts");
        Assert.Equal("Found 3 accounts", LoadFirst(json).STIGS.iSTIG.VULN[0].FINDING_DETAILS);
    }

    [Fact]
    public void Utf8Bom_IsTolerated()
    {
        Assert.Single(CklbConverter.ToCklList("﻿" + SingleStig));
    }

    [Fact]
    public void NonCklbJson_IsRejected()
    {
        Assert.Throws<FormatException>(() => CklbConverter.ToCklList(@"{ ""hello"": ""world"" }"));
        Assert.Throws<FormatException>(() => CklbConverter.ToCklList("<CHECKLIST></CHECKLIST>"));
        Assert.Throws<FormatException>(() => CklbConverter.ToCklList(@"{ ""stigs"": [ { ""rules"": [] } ] }"));
    }

    [Theory]
    [InlineData("not_a_finding", "NotAFinding")]
    [InlineData("Open", "Open")]
    [InlineData("not_applicable", "Not_Applicable")]
    [InlineData("not_reviewed", "Not_Reviewed")]
    [InlineData("", "Not_Reviewed")]
    [InlineData("something-else", "Not_Reviewed")]
    public void NormalizeStatus_MapsKnownValues(string input, string expected)
    {
        Assert.Equal(expected, CklbConverter.NormalizeStatus(input));
    }
}
