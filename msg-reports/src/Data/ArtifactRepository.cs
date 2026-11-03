// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.

using openrmf_msg_report.Models;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;

namespace openrmf_msg_report.Data {
    public class ArtifactRepository : IArtifactRepository
    {
        private readonly SystemGroupContext _context = null;

        public ArtifactRepository(Settings settings)
        {
            _context = new SystemGroupContext(settings);
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
        public async Task<Artifact> GetArtifact(string id)
        {
                return await _context.Artifacts.Find(artifact => artifact.InternalId == GetInternalId(id)).FirstOrDefaultAsync();
        }
    
        #region Systems

        public async Task<IEnumerable<Artifact>> GetSystemArtifacts(string systemGroupId)
        {
                var query = await _context.Artifacts.FindAsync(artifact => artifact.systemGroupId == systemGroupId);
                return query.ToList();
        }
        #endregion
    }
}