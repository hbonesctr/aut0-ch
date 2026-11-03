// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.

using openrmf_report_api.Models;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;

namespace openrmf_report_api.Data {
    public class ReportRepository : IReportRepository
    {
        private readonly ReportContext _context = null;

        public ReportRepository(IOptions<Settings> settings)
        {
            _context = new ReportContext(settings);
        }

        private ObjectId GetInternalId(string id)
        {
            ObjectId internalId;
            if (!ObjectId.TryParse(id, out internalId))
                internalId = ObjectId.Empty;

            return internalId;
        }

        // query after Id or InternalId (BSonId value)
        //
        public async Task<IEnumerable<NessusPatchData>> GetPatchDataBySystem(string id)
        {
                return await _context.ACASScanReports.Find(data => data.systemGroupId == id).ToListAsync();
        }

        public async Task<IEnumerable<VulnerabilityReport>> GetChecklistVulnerabilityData(string systemGroupId, string vulnid){
            return await _context.VulnerabilityReports.Find(v => v.vulnid.Contains(vulnid) && v.systemGroupId == systemGroupId).ToListAsync();
        }

        public async Task<List<VulnerabilityReport>> GetSystemVulnerabilityData(string systemGroupId, List<string> severity, List<string> status) {
            var filter = Builders<VulnerabilityReport>.Filter.Eq(x => x.systemGroupId, systemGroupId); // match on system group Id
            filter = filter & Builders<VulnerabilityReport>.Filter.In(x => x.status, status); // status has to be in the listing
            filter = filter & Builders<VulnerabilityReport>.Filter.In(x => x.severity, severity); // severity has to be in the listing
            var query = _context.VulnerabilityReports.AsQueryable().Where(_ => filter.Inject());
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<VulnerabilityReport>> GetChecklistVulnerabilityOverrideData(string systemGroupId) {
            
            var filter = Builders<VulnerabilityReport>.Filter.Eq(x => x.systemGroupId, systemGroupId);
            filter = filter & Builders<VulnerabilityReport>.Filter.Ne(x => x.severityOverride, ""); // severity override is filled in
            filter = filter & Builders<VulnerabilityReport>.Filter.Ne(x => x.severityOverride, null); // severity override is filled in
            return await _context.VulnerabilityReports.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<VulnerabilityReport>> GetChecklistVulnerabilityMissingKeyData(string systemGroupId) {
            
            // var filter = Builders<VulnerabilityReport>.Filter.Eq(x => x.systemGroupId, systemGroupId);
            // filter = filter & Builders<VulnerabilityReport>.Filter.Eq(x => x.comments, ""); // comments field empty
            // filter = filter & Builders<VulnerabilityReport>.Filter.Eq(x => x.details, ""); // finding details field empty
            return await _context.VulnerabilityReports.Find(x => x.systemGroupId == systemGroupId && 
                string.IsNullOrEmpty(x.comments) && string.IsNullOrEmpty(x.details) && 
                (x.status.ToLower() == "notafinding" || x.status.ToLower() == "not_applicable")).ToListAsync();
        }

        // check that the database is responding and it returns at least one collection name
        public bool HealthStatus(){
            var result = _context.ACASScanReports.Database.ListCollectionNamesAsync().GetAwaiter().GetResult().FirstOrDefault();
            if (!string.IsNullOrEmpty(result)) // we are good to go
                return true;
            return false;
        }
    }
}