// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using NATS.Client;
using System.Text;
using NLog;
using NLog.Config;
using openrmf_msg_report.Models;
using openrmf_msg_report.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using openrmf_msg_report.Classes;
using Newtonsoft.Json;
using System.Linq;

namespace openrmf_msg_report
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure MongoDB GUID serialization for legacy compatibility
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

            LogManager.Configuration = new XmlLoggingConfiguration($"{AppContext.BaseDirectory}nlog.config");

            var logger = LogManager.GetLogger("openrmf_msg_report");
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOGLEVEL"))) // default
                LogManager.Configuration.Variables["logLevel"] = "Warn";
            else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOGLEVEL"))) {
                switch (Environment.GetEnvironmentVariable("LOGLEVEL"))
                {
                    case "5":
                        LogManager.Configuration.Variables["logLevel"] = "Critical";
                        break;
                    case "4":
                        LogManager.Configuration.Variables["logLevel"] = "Error";
                        break;
                    case "3":
                        LogManager.Configuration.Variables["logLevel"] = "Warn";
                        break;
                    case "2":
                        LogManager.Configuration.Variables["logLevel"] = "Info";
                        break;
                    case "1":
                        LogManager.Configuration.Variables["logLevel"] = "Debug";
                        break;
                    case "0":
                        LogManager.Configuration.Variables["logLevel"] = "Trace";
                        break;
                    default:
                        LogManager.Configuration.Variables["logLevel"] = "Warn";
                        break;
                }
            }
            LogManager.ReconfigExistingLoggers();
            
            // Create a new connection factory to create a connection.
            ConnectionFactory cf = new ConnectionFactory();
            // add the options for the server, reconnecting, and the handler events
            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.MaxReconnect = -1;
            opts.ReconnectWait = 2000;
            opts.Name = "openrmf_msg_report";
            opts.Url = Environment.GetEnvironmentVariable("NATSSERVERURL");
            opts.AsyncErrorEventHandler += (sender, events) =>
            {
                logger.Info("NATS client error. Server: {0}. Message: {1}. Subject: {2}", events.Conn.ConnectedUrl, events.Error, events.Subscription.Subject);
            };

            opts.ServerDiscoveredEventHandler += (sender, events) =>
            {
                logger.Info("A new server has joined the cluster: {0}", events.Conn.DiscoveredServers);
            };

            opts.ClosedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Closed: {0}", events.Conn.ConnectedUrl);
            };

            opts.ReconnectedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Reconnected: {0}", events.Conn.ConnectedUrl);
            };

            opts.DisconnectedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Disconnected: {0}", events.Conn.ConnectedUrl);
            };
            
            // Creates a live connection to the NATS Server with the above options
            IConnection c = cf.CreateConnection(opts);

            // update all report data when a system is deleted
            // openrmf.system.delete -- delete all system data for reporting based on the payload of System ID
            // get the ID, query for the system, delete all data
            EventHandler<MsgHandlerEventArgs> deleteSystemData = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("NATS Delete all System Data: {0}", natsargs.Message.Subject);
                    logger.Info("NATS System Group ID: {0}",Encoding.UTF8.GetString(natsargs.Message.Data));
                    
                    // setup the MondoDB connection
                    Settings s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REPORTDBTYPE")) || Environment.GetEnvironmentVariable("REPORTDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("REPORTDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("REPORTDB");
                    }
                    // setup the database repo for reports to delete from
                    ReportRepository _reportRepo = new ReportRepository(s);
                    string systemGroupId = Encoding.UTF8.GetString(natsargs.Message.Data);
                    if (!string.IsNullOrEmpty(systemGroupId) ) {
                        bool deleted = _reportRepo.DeletePatchScanDataBySystemGroup(systemGroupId).Result;
                        if (deleted) {
                            logger.Info("Successfully deleted the system patch scan data for System Group {0}", systemGroupId);
                        } 
                        else {
                            logger.Warn("Did not delete the system patch scan data for System Group {0}. Maybe there is no data yet?", systemGroupId);
                        }
                    }
                    else {
                        logger.Warn("Warning: No System Group ID passed in when deleting System Report Patch Data system {0}", natsargs.Message.Subject);
                    }
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error retrieving system group record for system group");
                }
            };

            // update all report data when a checklist is deleted
            // openrmf.system.delete -- delete all system data for reporting based on the payload of System ID
            // get the ID, query for the system, delete all data
            EventHandler<MsgHandlerEventArgs> deleteChecklistVulnerabilities = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("NATS Delete Checklist Vulnerabilities: {0}", natsargs.Message.Subject);
                    logger.Info("NATS Checklist ID data: {0}",Encoding.UTF8.GetString(natsargs.Message.Data));
                    
                    // setup the MondoDB connection
                    Settings s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REPORTDBTYPE")) || Environment.GetEnvironmentVariable("REPORTDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("REPORTDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("REPORTDB");
                    }
                    // setup the database repo for reports to delete from
                    ReportRepository _reportRepo = new ReportRepository(s);
                    string artifactId = Encoding.UTF8.GetString(natsargs.Message.Data);
                    if (!string.IsNullOrEmpty(artifactId) ) {
                        bool deleted = _reportRepo.DeleteChecklistVulnerabilityData(artifactId).Result;
                        if (deleted) {
                            logger.Info("Successfully deleted the checklist vulnerability data for Artifact Id {0}", artifactId);
                        } 
                        else {
                            logger.Warn("Did not delete the checklist vulnerability data for Artifact Id {0}. Maybe there is no data yet?", artifactId);
                        }
                    }
                    else {
                        logger.Warn("Warning: No Artifact ID {0} passed in when deleting Checklist Vulnerability Data", natsargs.Message.Subject);
                    }
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error retrieving system group record for system group");
                }
            };

            // update the Nessus ACAS Patch Data listing in the database
            // openrmf.system.patchscan -- take the system group ID from the data and process away
            EventHandler<MsgHandlerEventArgs> updateSystemPatchScanData = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("NATS Update Nessus Patch Data: {0}", natsargs.Message.Subject);
                    logger.Info("NATS System Group ID: {0}",Encoding.UTF8.GetString(natsargs.Message.Data));
                    
                    SystemGroup sg;
                    // setup the MondoDB connection
                    Settings s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEMDBTYPE")) || Environment.GetEnvironmentVariable("SYSTEMDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("SYSTEMDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("SYSTEMDB");
                    }
                    // setup the database repo
                    SystemGroupRepository _systemGroupRepo = new SystemGroupRepository(s);
                    sg = _systemGroupRepo.GetSystemGroup(Encoding.UTF8.GetString(natsargs.Message.Data)).Result;
                    if (sg != null) {
                        // use the Report database connection
                        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REPORTDBTYPE")) || Environment.GetEnvironmentVariable("REPORTDBTYPE").ToLower() == "mongo") {
                            s.ConnectionString = Environment.GetEnvironmentVariable("REPORTDBCONNECTION");
                            s.Database = Environment.GetEnvironmentVariable("REPORTDB");
                        }
                        ReportRepository _reportRepo = new ReportRepository(s);
                        if (!string.IsNullOrEmpty(sg.rawNessusFile)) {
                            List<NessusPatchData> patchDataList = NessusPatchLoader.LoadPatchData(sg.rawNessusFile);
                            if (patchDataList != null && patchDataList.Count > 0) {
                                // delete the old data
                                bool deleted = _reportRepo.DeletePatchScanDataBySystemGroup(sg.InternalId.ToString()).Result;
                                // put in all the new data
                                foreach (NessusPatchData data in patchDataList) {
                                    data.systemGroupId = sg.InternalId.ToString();
                                }
                                var result = _reportRepo.AddPatchScanDataBulk(patchDataList).Result; 
                                if (result != null) {
                                    logger.Info("Report Message Client: Added scan plugin data for system group {0}", sg.InternalId.ToString());
                                }
                            } else {
                                logger.Warn("Warning: Nessus Data loading was empty for System Group {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                            }
                        } else {
                            logger.Warn("Warning: Nessus Data is empty for System Group {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                        }
                    } 
                    else {
                        logger.Warn("Warning: bad System Group ID when updating the patch data {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                    }
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error retrieving system group record for system group id {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                }
            };
            
            // Setup a new Score record based on a new checklist uploaded
            // This is called from the Upload API to say "hey I have a new checklist, score it"
            EventHandler<MsgHandlerEventArgs> newChecklistVulnerabilities = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("New NATS subject: {0}", natsargs.Message.Subject);
                    logger.Info("New NATS data: {0}",Encoding.UTF8.GetString(natsargs.Message.Data));
                    Settings s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEMDBTYPE")) || Environment.GetEnvironmentVariable("SYSTEMDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("SYSTEMDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("SYSTEMDB");
                    }
                    // setup the database repo
                    ArtifactRepository _artifactRepo = new ArtifactRepository(s);
                    // get the checklist
                    Artifact checklist =  _artifactRepo.GetArtifact(Encoding.UTF8.GetString(natsargs.Message.Data)).Result;
                    if (!string.IsNullOrEmpty(checklist.rawChecklist))
                        checklist.CHECKLIST = ChecklistLoader.LoadChecklist(checklist.rawChecklist);
                    // process it
                    if (checklist != null && checklist.CHECKLIST != null) {    
                        // check for is web or database v1.12
                        bool bWebDB = false;
                        string site = "";
                        string instance = "";
                        // we only need to figure this out one time
                        if (!string.IsNullOrWhiteSpace(checklist.CHECKLIST.ASSET.WEB_OR_DATABASE) && checklist.CHECKLIST.ASSET.WEB_OR_DATABASE == "true") {
                            bWebDB = true;                                
                            if (!string.IsNullOrWhiteSpace(checklist.CHECKLIST.ASSET.WEB_DB_SITE))
                                site = checklist.CHECKLIST.ASSET.WEB_DB_SITE.Trim();
                            if (!string.IsNullOrWhiteSpace(checklist.CHECKLIST.ASSET.WEB_DB_INSTANCE))
                                    instance = checklist.CHECKLIST.ASSET.WEB_DB_INSTANCE.Trim();
                        }
                                            
                        List<VulnerabilityReport> vulnReport =  new List<VulnerabilityReport>(); // put all findings into a list and roll out
                        VulnerabilityReport vulnRecord; // put the individual record into
                        foreach (VULN vulnerability in checklist.CHECKLIST.STIGS.iSTIG.VULN) {

                            // grab pertinent information
                            vulnRecord = new VulnerabilityReport();
                            // generated above just use the data
                            vulnRecord.isWebDatabase = bWebDB;
                            vulnRecord.webDatabaseSite = site;
                            vulnRecord.webDatabaseInstance = instance;
                            vulnRecord.systemGroupId = checklist.systemGroupId;
                            vulnRecord.artifactId = checklist.InternalId.ToString();
                            vulnRecord.created = checklist.created;
                            vulnRecord.createdBy = checklist.createdBy;
                            vulnRecord.vulnid = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Vuln_Num").FirstOrDefault().ATTRIBUTE_DATA;
                            logger.Info("Getting Artifact {2} data for newChecklistVulnerabilities(system: {0}, vulnid: {1}) successfully", 
                                checklist.systemGroupId, vulnRecord.vulnid, checklist.InternalId.ToString());

                            // get the hostname from the ASSET record
                            if (!string.IsNullOrEmpty(checklist.CHECKLIST.ASSET.HOST_NAME)) 
                                vulnRecord.hostname = checklist.CHECKLIST.ASSET.HOST_NAME;
                            else 
                                vulnRecord.hostname = "Unknown";

                            // start getting the vulnerability detailed information
                            vulnRecord.vulnid = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Vuln_Num").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.checklistVersion = checklist.CHECKLIST.STIGS.iSTIG.STIG_INFO.SI_DATA.Where(x => x.SID_NAME == "version").FirstOrDefault().SID_DATA;
                            vulnRecord.checklistRelease = checklist.CHECKLIST.STIGS.iSTIG.STIG_INFO.SI_DATA.Where(x => x.SID_NAME == "releaseinfo").FirstOrDefault().SID_DATA;
                            vulnRecord.checklistType = checklist.CHECKLIST.STIGS.iSTIG.STIG_INFO.SI_DATA.Where(x => x.SID_NAME == "title").FirstOrDefault().SID_DATA;
                            if (!string.IsNullOrEmpty(vulnRecord.checklistType)) {
                                vulnRecord.checklistType = ChecklistLoader.SanitizeChecklistType(vulnRecord.checklistType);
                            }
                            if (!string.IsNullOrEmpty(vulnRecord.checklistRelease)) {
                                vulnRecord.checklistRelease = ChecklistLoader.SanitizeChecklistRelease(vulnRecord.checklistRelease);
                            }
                            vulnRecord.comments = vulnerability.COMMENTS;
                            vulnRecord.details = vulnerability.FINDING_DETAILS;
                            vulnRecord.checkContent = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Check_Content").FirstOrDefault().ATTRIBUTE_DATA;                                
                            vulnRecord.discussion = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Vuln_Discuss").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.fixText = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Fix_Text").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.ruleTitle = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Rule_Title").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.severity = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Severity").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.status = vulnerability.STATUS;
                            // get all the list of CCIs
                            foreach(STIG_DATA stig in vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "CCI_REF").ToList()) {
                                // add each one of these, from 0 to N of them
                                if (!string.IsNullOrEmpty(stig.ATTRIBUTE_DATA)) vulnRecord.cciList.Add(stig.ATTRIBUTE_DATA);
                            }
                            logger.Info("Adding Artifact {2} to the list for newChecklistVulnerabilities (system: {0}, vulnid: {1}) successfully", 
                                checklist.systemGroupId, vulnRecord.vulnid, checklist.InternalId.ToString());
                            vulnReport.Add(vulnRecord); // add it to the listing

                        } // for each VULN record
                        // save every single VULN record with the vuln number, artifactId and systemGroupId into the database

                        // setup the MondoDB connection
                        s = new Settings();
                        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REPORTDBTYPE")) || Environment.GetEnvironmentVariable("REPORTDBTYPE").ToLower() == "mongo") {
                            s.ConnectionString = Environment.GetEnvironmentVariable("REPORTDBCONNECTION");
                            s.Database = Environment.GetEnvironmentVariable("REPORTDB");
                        }
                        // setup the database repo for reports to delete from
                        ReportRepository _reportRepo = new ReportRepository(s);
                        var result = _reportRepo.AddChecklistVulnerabilityDataBulk(vulnReport);
                        if (result != null) 
                            logger.Info("Added vulnerability information on system package {0} checklist {1} vulnerabilities", checklist.systemGroupId, checklist.InternalIdString);
                        // }
                    }
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error saving new scoring information for artifactId {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                }
            };

            // Update Checklist Vulnerability records based on an updated checklist uploaded
            // This is called from the Save API to say "hey I have updated a VULN record, you may want to update your reports"
            EventHandler<MsgHandlerEventArgs> updateChecklistVulnerabilities = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info(natsargs.Message.Subject);
                    logger.Info(Encoding.UTF8.GetString(natsargs.Message.Data));
                    // setup the Checklist database repo
                    Settings s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEMDBTYPE")) || Environment.GetEnvironmentVariable("SYSTEMDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("SYSTEMDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("SYSTEMDB");
                    }
                    ArtifactRepository _artifactRepo = new ArtifactRepository(s);
                    // setup the database repo for reports to delete from
                    // setup the MondoDB connection
                    s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REPORTDBTYPE")) || Environment.GetEnvironmentVariable("REPORTDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("REPORTDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("REPORTDB");
                    }
                    ReportRepository _reportRepo = new ReportRepository(s);
                    // get the checklist
                    Artifact checklist = _artifactRepo.GetArtifact(Encoding.UTF8.GetString(natsargs.Message.Data)).Result;

                    if (checklist.CHECKLIST == null)
                        checklist.CHECKLIST = ChecklistLoader.LoadChecklist(checklist.rawChecklist);
                    if (checklist.CHECKLIST.STIGS.iSTIG.VULN.Count == 0)
                        checklist.CHECKLIST = ChecklistLoader.LoadChecklist(checklist.rawChecklist);
                    // process it
                    if (checklist != null && checklist.CHECKLIST != null) {  
                        // check for is web or database v1.12
                        bool bWebDB = false;
                        string site = "";
                        string instance = "";
                        // we only need to figure this out one time
                        if (!string.IsNullOrWhiteSpace(checklist.CHECKLIST.ASSET.WEB_OR_DATABASE) && checklist.CHECKLIST.ASSET.WEB_OR_DATABASE == "true") {
                            bWebDB = true;                                
                            if (!string.IsNullOrWhiteSpace(checklist.CHECKLIST.ASSET.WEB_DB_SITE))
                                site = checklist.CHECKLIST.ASSET.WEB_DB_SITE.Trim();
                            if (!string.IsNullOrWhiteSpace(checklist.CHECKLIST.ASSET.WEB_DB_INSTANCE))
                                    instance = checklist.CHECKLIST.ASSET.WEB_DB_INSTANCE.Trim();
                        }
                        List<VulnerabilityReport> vulnReport =  new List<VulnerabilityReport>(); // put all findings into a list and roll out
                        VulnerabilityReport vulnRecord; // put the individual record into
                        foreach (VULN vulnerability in checklist.CHECKLIST.STIGS.iSTIG.VULN) {
                            // grab pertinent information
                            vulnRecord = new VulnerabilityReport();
                            // generated above just use the data
                            vulnRecord.isWebDatabase = bWebDB;
                            vulnRecord.webDatabaseSite = site;
                            vulnRecord.webDatabaseInstance = instance;
                            vulnRecord.systemGroupId = checklist.systemGroupId;
                            vulnRecord.artifactId = checklist.InternalId.ToString();
                            vulnRecord.vulnid = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Vuln_Num").FirstOrDefault().ATTRIBUTE_DATA;
                            logger.Info("Getting Artifact {2} data for updateChecklistVulnerabilities(system: {0}, vulnid: {1}) successfully", checklist.systemGroupId, vulnRecord.vulnid, checklist.InternalId.ToString());

                            // get the hostname from the ASSET record
                            if (!string.IsNullOrEmpty(checklist.CHECKLIST.ASSET.HOST_NAME)) 
                                vulnRecord.hostname = checklist.CHECKLIST.ASSET.HOST_NAME;
                            else 
                                vulnRecord.hostname = "Unknown";

                            // start getting the vulnerability detailed information
                            vulnRecord.vulnid = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Vuln_Num").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.checklistVersion = checklist.CHECKLIST.STIGS.iSTIG.STIG_INFO.SI_DATA.Where(x => x.SID_NAME == "version").FirstOrDefault().SID_DATA;
                            vulnRecord.checklistRelease = checklist.CHECKLIST.STIGS.iSTIG.STIG_INFO.SI_DATA.Where(x => x.SID_NAME == "releaseinfo").FirstOrDefault().SID_DATA;
                            vulnRecord.checklistType = checklist.CHECKLIST.STIGS.iSTIG.STIG_INFO.SI_DATA.Where(x => x.SID_NAME == "title").FirstOrDefault().SID_DATA;
                            if (!string.IsNullOrEmpty(vulnRecord.checklistType)) {
                                vulnRecord.checklistType = ChecklistLoader.SanitizeChecklistType(vulnRecord.checklistType);
                            }
                            if (!string.IsNullOrEmpty(vulnRecord.checklistRelease)) {
                                vulnRecord.checklistRelease = ChecklistLoader.SanitizeChecklistRelease(vulnRecord.checklistRelease);
                            }
                            vulnRecord.comments = vulnerability.COMMENTS;
                            vulnRecord.details = vulnerability.FINDING_DETAILS;
                            vulnRecord.checkContent = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Check_Content").FirstOrDefault().ATTRIBUTE_DATA;                                
                            vulnRecord.discussion = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Vuln_Discuss").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.fixText = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Fix_Text").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.ruleTitle = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Rule_Title").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.severity = vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "Severity").FirstOrDefault().ATTRIBUTE_DATA;
                            vulnRecord.status = vulnerability.STATUS;
                            vulnRecord.created = checklist.created;
                            vulnRecord.createdBy = checklist.createdBy;
                            vulnRecord.updatedBy = checklist.updatedBy;
                            vulnRecord.updatedOn = checklist.updatedOn;
                            // get all the list of CCIs
                            foreach(STIG_DATA stig in vulnerability.STIG_DATA.Where(cc => cc.VULN_ATTRIBUTE == "CCI_REF").ToList()) {
                                // add each one of these, from 0 to N of them
                                if (!string.IsNullOrEmpty(stig.ATTRIBUTE_DATA)) vulnRecord.cciList.Add(stig.ATTRIBUTE_DATA);
                            }
                            logger.Info("Adding Artifact {2} to the list for updateChecklistVulnerabilities (system: {0}, vulnid: {1}) successfully", checklist.systemGroupId, vulnRecord.vulnid, checklist.InternalId.ToString());
                            vulnReport.Add(vulnRecord); // add it to the listing
                        } // for each VULN record
                        // save every single VULN record with the vuln number, artifactId and systemGroupId into the database

                        bool result;
                        // delete all the checklist report records as we only report on the latest and greatest ones
                        result = _reportRepo.DeleteChecklistVulnerabilityData(checklist.InternalIdString).Result;
                        if (result) {
                            logger.Info("Successfully removed old vulnerability report data on system {0} checklist {1}", checklist.systemGroupId, checklist.InternalIdString);
                        }
                        else {
                            logger.Warn("Error removing old vulnerability report data on system {0} checklist {1}", checklist.systemGroupId, checklist.InternalIdString);
                        }
                        var bulkResult = _reportRepo.AddChecklistVulnerabilityDataBulk(vulnReport);
                        if (bulkResult != null) 
                            logger.Info("Added vulnerability information on system package {0} checklist {1} vulnerabilities", checklist.systemGroupId, checklist.InternalIdString);
                    }
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error saving updated scoring information for artifactId {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                }
            };

            // update the Nessus ACAS Patch Data listing in the database
            // openrmf.report.refresh.nessuspatchdata
            EventHandler<MsgHandlerEventArgs> refreshNessusPatchData = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("NATS Report Refresh Nessus Patch Data: {0}", natsargs.Message.Subject);
                    IEnumerable<SystemGroup> systems;
                    // setup the MondoDB connection
                    Settings s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEMDBTYPE")) || Environment.GetEnvironmentVariable("SYSTEMDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("SYSTEMDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("SYSTEMDB");
                    }
                    // setup the database repo
                    SystemGroupRepository _systemGroupRepo = new SystemGroupRepository(s);
                    systems = _systemGroupRepo.GetAllSystemGroups().Result;
                    if (systems != null && systems.Count() > 0) {
                        // use the Report database connection
                        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REPORTDBTYPE")) || Environment.GetEnvironmentVariable("REPORTDBTYPE").ToLower() == "mongo") {
                            s.ConnectionString = Environment.GetEnvironmentVariable("REPORTDBCONNECTION");
                            s.Database = Environment.GetEnvironmentVariable("REPORTDB");
                        }
                        ReportRepository _reportRepo = new ReportRepository(s);
                        foreach(SystemGroup sg in systems) {
                            if (!string.IsNullOrEmpty(sg.rawNessusFile)) {
                                List<NessusPatchData> patchDataList = NessusPatchLoader.LoadPatchData(sg.rawNessusFile);
                                if (patchDataList != null && patchDataList.Count > 0) {
                                    // delete the old data
                                    bool deleted = _reportRepo.DeletePatchScanDataBySystemGroup(sg.InternalId.ToString()).Result;
                                    // put in all the new data
                                    foreach (NessusPatchData data in patchDataList) {
                                        data.systemGroupId = sg.InternalId.ToString();
                                    }
                                    var result = _reportRepo.AddPatchScanDataBulk(patchDataList).Result; 
                                    if (result != null) {
                                        logger.Info("Report Message Client: Added scan plugin data for system group {0}", sg.InternalId.ToString());
                                    }
                                } else {
                                    logger.Warn("Warning: Nessus Data loading was empty for refreshNessusPatchData System Group {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                                }
                            } else {
                                logger.Warn("Warning: Nessus Data is empty for refreshNessusPatchData System Group {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                            }
                        }
                    } 
                    else {
                        logger.Warn("Warning: bad no System Group Ids in refreshNessusPatchData");
                    }
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error retrieving all system group records for refreshNessusPatchData");
                }
            };

            // Setup an updated Score record based on an updated checklist uploaded
            // This is called from the Upload API to say "hey I have an updated checklist, you may want to update your scoring"
            EventHandler<MsgHandlerEventArgs> updateSingleVulnerability = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info(natsargs.Message.Subject);
                    logger.Info(Encoding.UTF8.GetString(natsargs.Message.Data));
                    // setup the Checklist database repo
                    Settings s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEMDBTYPE")) || Environment.GetEnvironmentVariable("SYSTEMDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("SYSTEMDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("SYSTEMDB");
                    }
                    ArtifactRepository _artifactRepo = new ArtifactRepository(s);
                    // setup the database repo for reports to delete from
                    // setup the MondoDB connection
                    s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REPORTDBTYPE")) || Environment.GetEnvironmentVariable("REPORTDBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("REPORTDBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("REPORTDB");
                    }
                    ReportRepository _reportRepo = new ReportRepository(s);
                    // get the data
                    Dictionary<string, string> vulnAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Compression.DecompressString(Encoding.UTF8.GetString(natsargs.Message.Data)));
                    if (vulnAttributes != null) {
                        Artifact checklist = _artifactRepo.GetArtifact(vulnAttributes["artifactId"]).Result;
                        // process it
                        if (checklist != null) {
                            // get the current vulnerability
                            VulnerabilityReport report = _reportRepo.GetChecklistVulnerabilityData(vulnAttributes["systemGroupId"], 
                                vulnAttributes["artifactId"], vulnAttributes["vulnId"]).Result;
                            // update its values
                            if (report != null) {
                                report.details = vulnAttributes["finding_details"];
                                report.status = vulnAttributes["status"];
                                report.comments = vulnAttributes["comments"];
                                report.severityOverride = vulnAttributes["severity_override"];
                                report.severityJustification = vulnAttributes["severity_justification"];
                                report.updatedBy = Guid.Parse(vulnAttributes["updatedBy"]);
                                report.updatedOn = DateTime.Now;
                                // clean up old data if needed
                                report.checklistType = ChecklistLoader.SanitizeChecklistType(report.checklistType);
                                report.checklistRelease = ChecklistLoader.SanitizeChecklistRelease(report.checklistRelease);
                                // save the data passed to us
                                bool updated = _reportRepo.UpdateChecklistVulnerabilityData(report).Result;
                            } else {
                                logger.Warn(string.Format("Vulnerability Record not found to update. Re-run vulnerability refresh for system {0} to ensure all data is accurate."),
                                    vulnAttributes["systemGroupId"]);
                                return;
                            }
                            logger.Info("Updating Vulnerability updateSingleVulnerability (system: {0}, checklist: {1}, vulnid: {2}) successfully", vulnAttributes["systemGroupId"], 
                                vulnAttributes["artifactId"], vulnAttributes["vulnId"]);
                        }
                    } // if vulnAttributes not null
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error saving updated scoring information for artifactId {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                }
            };

            // setup of all subscriber agents and methods
            logger.Info("Report Message Client: setting up the OpenRMF System Delete for Report subscription");
            IAsyncSubscription asyncSystemDelete = c.SubscribeAsync("openrmf.system.delete", deleteSystemData);
            logger.Info("Report Message Client: setting up the OpenRMF Nessus ACAS Patch Scan for Report subscription");
            IAsyncSubscription asyncSystemPatchScan = c.SubscribeAsync("openrmf.system.patchscan", updateSystemPatchScanData);
            logger.Info("Report Message Client: setting up the OpenRMF new vulnerabilities subscriptions");
            IAsyncSubscription asyncNewChecklist = c.SubscribeAsync("openrmf.checklist.save.new", newChecklistVulnerabilities);
            logger.Info("Report Message Client: setting up the OpenRMF update vulnerabilities subscriptions");
            IAsyncSubscription asyncUpdateChecklist = c.SubscribeAsync("openrmf.checklist.save.update", updateChecklistVulnerabilities);
            logger.Info("Report Message Client: setting up the OpenRMF delete checklist vulnerabilities subscriptions");
            IAsyncSubscription asyncChecklistDelete = c.SubscribeAsync("openrmf.checklist.delete", deleteChecklistVulnerabilities);
            logger.Info("Report Message Client: setting up the OpenRMF update single vulnerability subscriptions");
            IAsyncSubscription asyncVulnerabilityUpdate = c.SubscribeAsync("openrmf.checklist.save.vulnerability.update", updateSingleVulnerability);

            // refresh data subscriptions
            logger.Info("Report Message Client: setting up the OpenRMF refresh Nessus Patch Data subscription");
            IAsyncSubscription asyncRefreshNessusPatchData = c.SubscribeAsync("openrmf.report.refresh.nessuspatchdata", refreshNessusPatchData);
        }

        private static ObjectId GetInternalId(string id)
        {
            ObjectId internalId;
            if (!ObjectId.TryParse(id, out internalId))
                internalId = ObjectId.Empty;
            return internalId;
        }
    }
}