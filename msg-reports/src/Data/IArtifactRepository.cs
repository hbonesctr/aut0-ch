// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using openrmf_msg_report.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace openrmf_msg_report.Data {
    public interface IArtifactRepository
    {
        // return checklist records for a given system
        Task<IEnumerable<Artifact>> GetSystemArtifacts(string system);

        // get a particular Artifact record
        Task<Artifact> GetArtifact(string id);
    }
}