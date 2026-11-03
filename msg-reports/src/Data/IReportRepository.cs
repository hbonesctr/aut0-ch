// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using openrmf_msg_report.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace openrmf_msg_report.Data {
    public interface IReportRepository
    {
        Task<IEnumerable<NessusPatchData>> GetAllPatchScanDataBySystemGroup(string systemGroupId);
        Task<NessusPatchData> AddPatchScanData(NessusPatchData data);
        Task<List<NessusPatchData>> AddPatchScanDataBulk(List<NessusPatchData> items);
        Task<bool> DeleteAllSystemData(string systemGroupId);
        Task<bool> DeletePatchScanDataBySystemGroup(string systemGroupId);
        Task<VulnerabilityReport> AddChecklistVulnerabilityData(VulnerabilityReport data);
        Task<List<VulnerabilityReport>> AddChecklistVulnerabilityDataBulk(List<VulnerabilityReport> data);
        Task<bool> UpdateChecklistVulnerabilityData(VulnerabilityReport data);
        Task<bool> DeleteChecklistVulnerabilityDataBySystemGroup(string systemGroupId);
        Task<bool> DeleteChecklistVulnerabilityData(string artifactId);
        Task<VulnerabilityReport> GetChecklistVulnerabilityData(string systemGroupId, string artifactId, string vulnid);
        Task<IEnumerable<VulnerabilityReport>> FindChecklistVulnerabilityData(string systemGroupId, string vulnid);
    }
}