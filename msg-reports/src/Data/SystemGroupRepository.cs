// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using openrmf_msg_report.Models;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace openrmf_msg_report.Data {
    public class SystemGroupRepository : ISystemGroupRepository
    {
        private readonly SystemGroupContext _context = null;

        public SystemGroupRepository(Settings settings)
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
        public async Task<SystemGroup> GetSystemGroup(string id)
        {
                ObjectId internalId = GetInternalId(id);
                return await _context.SystemGroups
                                .Find(SystemGroup => SystemGroup.InternalId == internalId).FirstOrDefaultAsync();
        }

        // query all System Group records
        public async Task<IEnumerable<SystemGroup>> GetAllSystemGroups()
        {
                return await _context.SystemGroups.Find(_ => true).ToListAsync();
        }

    }
}