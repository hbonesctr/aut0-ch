// Copyright (c) 2026 Hector L. Bones. Part of AUT0-CH, a modified version of OpenRMF OSS.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace openrmf_read_api.Classes
{
    /// <summary>
    /// Converts a STIG Viewer 3 checklist (.cklb, JSON) into STIG Viewer 2 checklist XML (.ckl)
    /// so the existing CKL pipeline (scoring, reports, compliance, exports) can process it unchanged.
    /// One CKL string is produced per STIG in the CKLB, because OpenRMF stores one STIG per checklist.
    /// Uses only the .NET base class library (no extra packages) so it builds offline.
    /// </summary>
    /// <remarks>
    /// CKLB-only data with no CKL equivalent is not carried over: checklist/STIG/rule UUIDs beyond
    /// STIG_UUID, group_tree, createdAt/updatedAt, and target comments.
    /// </remarks>
    public static class CklbConverter
    {
        /// <summary>Quick check used by the upload routine to reject non-CKLB JSON early.</summary>
        public static bool LooksLikeCklb(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var trimmed = raw.TrimStart('﻿', ' ', '\t', '\r', '\n');
            return trimmed.StartsWith("{") && raw.IndexOf("\"stigs\"", StringComparison.OrdinalIgnoreCase) > 0;
        }

        /// <summary>
        /// Convert a CKLB JSON document into one CKL XML string per STIG it contains.
        /// Throws <see cref="FormatException"/> if the JSON is not a usable CKLB.
        /// </summary>
        public static List<string> ToCklList(string cklbJson)
        {
            if (!LooksLikeCklb(cklbJson))
                throw new FormatException("The file is not a STIG Viewer 3 CKLB checklist (no 'stigs' array found).");

            var results = new List<string>();
            using (JsonDocument doc = JsonDocument.Parse(cklbJson.TrimStart('﻿'), new JsonDocumentOptions {
                AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip }))
            {
                JsonElement root = doc.RootElement;
                JsonElement target = Prop(root, "target_data");
                if (!TryProp(root, "stigs", out JsonElement stigs) || stigs.ValueKind != JsonValueKind.Array)
                    throw new FormatException("The CKLB 'stigs' property is missing or is not an array.");

                foreach (JsonElement stig in stigs.EnumerateArray())
                {
                    if (!TryProp(stig, "rules", out JsonElement rules) || rules.ValueKind != JsonValueKind.Array || rules.GetArrayLength() == 0)
                        continue; // nothing to score
                    results.Add(BuildCkl(target, stig, rules));
                }
            }
            if (results.Count == 0)
                throw new FormatException("The CKLB file contains no STIGs with rules.");
            return results;
        }

        private static string BuildCkl(JsonElement target, JsonElement stig, JsonElement rules)
        {
            string stigTitle = FirstNonEmpty(Str(stig, "stig_name"), Str(stig, "display_name"), Str(stig, "title"));
            string version = Str(stig, "version");
            string releaseInfo = Str(stig, "release_info");
            string stigUuid = Str(stig, "uuid");
            string stigRef = string.IsNullOrEmpty(version)
                ? stigTitle + (string.IsNullOrEmpty(releaseInfo) ? "" : " :: " + releaseInfo)
                : $"{stigTitle} :: Version {version}, {releaseInfo}".TrimEnd(' ', ',');

            // first non-empty target_key on a rule is the checklist TARGET_KEY
            string targetKey = rules.EnumerateArray().Select(r => Str(r, "target_key")).FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? "";

            var sb = new StringBuilder();
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = false, Encoding = Encoding.UTF8 };
            using (var sw = new StringWriter(sb))
            using (XmlWriter w = XmlWriter.Create(sw, settings))
            {
                w.WriteStartElement("CHECKLIST");

                // ---- ASSET ----
                w.WriteStartElement("ASSET");
                El(w, "ROLE", FirstNonEmpty(Str(target, "role"), "None"));
                El(w, "ASSET_TYPE", FirstNonEmpty(Str(target, "target_type"), "Computing"));
                El(w, "MARKING", FirstNonEmpty(Str(target, "marking"), Str(target, "classification"), Str(stig, "classification")));
                El(w, "HOST_NAME", Str(target, "host_name"));
                El(w, "HOST_IP", Str(target, "ip_address"));
                El(w, "HOST_MAC", Str(target, "mac_address"));
                El(w, "HOST_FQDN", Str(target, "fqdn"));
                El(w, "TARGET_COMMENT", Str(target, "comments"));
                El(w, "TECH_AREA", Str(target, "technology_area"));
                El(w, "TARGET_KEY", targetKey);
                El(w, "WEB_OR_DATABASE", Bool(target, "is_web_database") ? "true" : "false");
                El(w, "WEB_DB_SITE", Str(target, "web_db_site"));
                El(w, "WEB_DB_INSTANCE", Str(target, "web_db_instance"));
                w.WriteEndElement();

                // ---- STIGS / iSTIG ----
                w.WriteStartElement("STIGS");
                w.WriteStartElement("iSTIG");

                w.WriteStartElement("STIG_INFO");
                SiData(w, "version", version);
                SiData(w, "classification", FirstNonEmpty(Str(stig, "classification"), "UNCLASSIFIED"));
                SiData(w, "customname", "");
                SiData(w, "stigid", Str(stig, "stig_id"));
                SiData(w, "description", Str(stig, "description"));
                SiData(w, "filename", Str(stig, "filename"));
                SiData(w, "releaseinfo", releaseInfo);
                SiData(w, "title", stigTitle);
                SiData(w, "uuid", stigUuid);
                SiData(w, "notice", "terms-of-use");
                SiData(w, "source", Str(stig, "reference_identifier"));
                w.WriteEndElement(); // STIG_INFO

                foreach (JsonElement rule in rules.EnumerateArray())
                    WriteVuln(w, rule, stigRef, stigUuid, targetKey);

                w.WriteEndElement(); // iSTIG
                w.WriteEndElement(); // STIGS
                w.WriteEndElement(); // CHECKLIST
            }
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + sb.ToString();
        }

        private static void WriteVuln(XmlWriter w, JsonElement rule, string stigRef, string stigUuid, string targetKey)
        {
            w.WriteStartElement("VULN");
            string ruleId = FirstNonEmpty(Str(rule, "rule_id_src"), Str(rule, "rule_id"));
            JsonElement ccr = Prop(rule, "check_content_ref");
            string checkRef = ccr.ValueKind == JsonValueKind.Object ? FirstNonEmpty(Str(ccr, "name"), Str(ccr, "href")) : Str(rule, "check_content_ref");

            StigData(w, "Vuln_Num", FirstNonEmpty(Str(rule, "group_id_src"), Str(rule, "group_id")));
            StigData(w, "Severity", NormalizeSeverity(Str(rule, "severity")));
            StigData(w, "Group_Title", Str(rule, "group_title"));
            StigData(w, "Rule_ID", ruleId);
            StigData(w, "Rule_Ver", Str(rule, "rule_version"));
            StigData(w, "Rule_Title", Str(rule, "rule_title"));
            // DISA ships "discussion" as the raw XCCDF description: <VulnDiscussion>text</VulnDiscussion><FalsePositives>...
            // CKL keeps only the VulnDiscussion text; the other embedded tags back-fill empty rule fields.
            string discussion = Str(rule, "discussion");
            string Field(string jsonName, string embeddedTag) => FirstNonEmpty(Str(rule, jsonName), EmbeddedTag(discussion, embeddedTag));
            StigData(w, "Vuln_Discuss", EmbeddedTag(discussion, "VulnDiscussion") ?? discussion);
            StigData(w, "IA_Controls", Field("ia_controls", "IAControls"));
            StigData(w, "Check_Content", Str(rule, "check_content"));
            StigData(w, "Fix_Text", Str(rule, "fix_text"));
            StigData(w, "False_Positives", Field("false_positives", "FalsePositives"));
            StigData(w, "False_Negatives", Field("false_negatives", "FalseNegatives"));
            StigData(w, "Documentable", FirstNonEmpty(Field("documentable", "Documentable"), "false"));
            StigData(w, "Mitigations", Field("mitigations", "Mitigations"));
            StigData(w, "Potential_Impact", Field("potential_impacts", "PotentialImpacts"));
            StigData(w, "Third_Party_Tools", Field("third_party_tools", "ThirdPartyTools"));
            StigData(w, "Mitigation_Control", Field("mitigation_control", "MitigationControl"));
            StigData(w, "Responsibility", Field("responsibility", "Responsibility"));
            StigData(w, "Security_Override_Guidance", Field("security_override_guidance", "SeverityOverrideGuidance"));
            StigData(w, "Check_Content_Ref", checkRef);
            StigData(w, "Weight", FirstNonEmpty(Str(rule, "weight"), "10.0"));
            StigData(w, "Class", NormalizeClass(Str(rule, "classification")));
            StigData(w, "STIGRef", stigRef);
            StigData(w, "TargetKey", FirstNonEmpty(Str(rule, "target_key"), targetKey));
            StigData(w, "STIG_UUID", FirstNonEmpty(Str(rule, "stig_uuid"), stigUuid));
            foreach (string legacy in StrArray(rule, "legacy_ids"))
                StigData(w, "LEGACY_ID", legacy);
            foreach (string cci in StrArray(rule, "ccis"))
                StigData(w, "CCI_REF", cci);

            ReadOverride(rule, out string sevOverride, out string sevJustification);
            El(w, "STATUS", NormalizeStatus(Str(rule, "status")));
            El(w, "FINDING_DETAILS", Str(rule, "finding_details"));
            El(w, "COMMENTS", Str(rule, "comments"));
            El(w, "SEVERITY_OVERRIDE", sevOverride);
            El(w, "SEVERITY_JUSTIFICATION", sevJustification);
            w.WriteEndElement(); // VULN
        }

        /// <summary>Map CKLB status values to the CKL values the rest of the application expects.</summary>
        public static string NormalizeStatus(string status)
        {
            switch ((status ?? "").Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_"))
            {
                case "not_a_finding":
                case "notafinding":
                case "pass":
                    return "NotAFinding";
                case "open":
                case "fail":
                    return "Open";
                case "not_applicable":
                case "notapplicable":
                case "na":
                    return "Not_Applicable";
                default:
                    return "Not_Reviewed";
            }
        }

        private static string NormalizeSeverity(string severity)
        {
            switch ((severity ?? "").Trim().ToLowerInvariant())
            {
                case "high": case "cat i": case "cat_i": return "high";
                case "low": case "cat iii": case "cat_iii": return "low";
                case "medium": case "cat ii": case "cat_ii": return "medium";
                default: return string.IsNullOrWhiteSpace(severity) ? "medium" : severity.Trim().ToLowerInvariant();
            }
        }

        /// <summary>STIG Viewer 2 writes the rule class as "Unclass"; CKLB uses "Unclassified".</summary>
        private static string NormalizeClass(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().StartsWith("unclass", StringComparison.OrdinalIgnoreCase))
                return "Unclass";
            return value.Trim();
        }

        /// <summary>
        /// Return the text between &lt;tag&gt; and &lt;/tag&gt; in a DISA XCCDF description string, or null if the tag
        /// is not there. Only the named tag is matched, so literal text such as "&lt;path&gt;" inside it is left alone.
        /// </summary>
        public static string EmbeddedTag(string text, string tag)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string open = "<" + tag + ">", close = "</" + tag + ">";
            int start = text.IndexOf(open, StringComparison.Ordinal);
            if (start < 0) return null;
            start += open.Length;
            int end = text.IndexOf(close, start, StringComparison.Ordinal);
            return end < 0 ? null : text.Substring(start, end - start).Trim();
        }

        /// <summary>
        /// CKLB keeps overrides in an "overrides" object. The severity override has been seen both as
        /// {"severity": {"severity": "low", "reason": "..."}} and as {"severity": "low", "reason": "..."}.
        /// </summary>
        private static void ReadOverride(JsonElement rule, out string severity, out string justification)
        {
            severity = ""; justification = "";
            JsonElement overrides = Prop(rule, "overrides");
            if (overrides.ValueKind != JsonValueKind.Object) return;
            JsonElement sev = Prop(overrides, "severity");
            if (sev.ValueKind == JsonValueKind.Object)
            {
                severity = Str(sev, "severity");
                justification = FirstNonEmpty(Str(sev, "reason"), Str(sev, "justification"));
            }
            else if (sev.ValueKind == JsonValueKind.String)
            {
                severity = sev.GetString();
                justification = FirstNonEmpty(Str(overrides, "reason"), Str(overrides, "justification"));
            }
            if (!string.IsNullOrEmpty(severity)) severity = NormalizeSeverity(severity);
        }

        // ---------- small JSON / XML helpers ----------

        private static bool TryProp(JsonElement e, string name, out JsonElement value)
        {
            value = default;
            if (e.ValueKind != JsonValueKind.Object) return false;
            if (e.TryGetProperty(name, out value)) return true;
            foreach (JsonProperty p in e.EnumerateObject())
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) { value = p.Value; return true; }
            return false;
        }

        private static JsonElement Prop(JsonElement e, string name) => TryProp(e, name, out JsonElement v) ? v : default;

        private static string Str(JsonElement e, string name)
        {
            if (!TryProp(e, name, out JsonElement v)) return "";
            switch (v.ValueKind)
            {
                case JsonValueKind.String: return v.GetString() ?? "";
                case JsonValueKind.Number: return v.GetRawText();
                case JsonValueKind.True: return "true";
                case JsonValueKind.False: return "false";
                default: return "";
            }
        }

        private static bool Bool(JsonElement e, string name)
        {
            if (!TryProp(e, name, out JsonElement v)) return false;
            if (v.ValueKind == JsonValueKind.True) return true;
            if (v.ValueKind == JsonValueKind.String) return string.Equals(v.GetString(), "true", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private static IEnumerable<string> StrArray(JsonElement e, string name)
        {
            if (!TryProp(e, name, out JsonElement v) || v.ValueKind != JsonValueKind.Array) yield break;
            foreach (JsonElement item in v.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                    yield return item.GetString().Trim();
        }

        private static string FirstNonEmpty(params string[] values) => values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? "";

        private static void El(XmlWriter w, string name, string value) => w.WriteElementString(name, XmlSafe(value));

        /// <summary>Drop characters that are illegal in XML 1.0 (JSON allows them, XmlWriter throws on them).</summary>
        private static string XmlSafe(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                {
                    sb.Append(c).Append(value[++i]);
                }
                else if (XmlConvert.IsXmlChar(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static void SiData(XmlWriter w, string name, string value)
        {
            w.WriteStartElement("SI_DATA");
            El(w, "SID_NAME", name);
            El(w, "SID_DATA", value);
            w.WriteEndElement();
        }

        private static void StigData(XmlWriter w, string attribute, string value)
        {
            w.WriteStartElement("STIG_DATA");
            El(w, "VULN_ATTRIBUTE", attribute);
            El(w, "ATTRIBUTE_DATA", value);
            w.WriteEndElement();
        }
    }
}
