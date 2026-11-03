// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.

using openrmf_report_api.Models;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace openrmf_report_api.Data {
    public interface IReportRepository
    {
        Task<IEnumerable<NessusPatchData>> GetPatchDataBySystem(string id);
        Task<IEnumerable<VulnerabilityReport>> GetChecklistVulnerabilityData(string systemGroupId, string vulnid);
        Task<List<VulnerabilityReport>> GetSystemVulnerabilityData(string systemGroupId, List<string> severity, List<string> status);
        Task<IEnumerable<VulnerabilityReport>> GetChecklistVulnerabilityOverrideData(string systemGroupId);
        Task<IEnumerable<VulnerabilityReport>> GetChecklistVulnerabilityMissingKeyData(string systemGroupId);
        bool HealthStatus();
    }
}