![.NET Core Build and Test](https://github.com/Cingulara/openrmf-msg-reports/workflows/.NET%20Core%20Build%20and%20Test/badge.svg)

# openrmf-msg-reports
Messaging service to respond to internal API requests to receive Nessus, artifact and checklist information using a NATS Request/Reply scenario.
* openrmf.system.delete - delete all reports for anything related to this system across all records and collections
* openrmf.system.patchscan - updated the ACAS Patch data for this system
* openrmf.checklist.save.new - add the vulnerability listing for this new checklist
* openrmf.checklist.save.update - update the vulnerability listing for this new checklist
* openrmf.checklist.delete - delete the vulnerability listing for this new checklist
* openrmf.report.refresh.vulnerabilitydata - refresh the vulnerability data for all checklists across all systems
* openrmf.report.refresh.nessuspatchdata - refresh the Nessus ACAS patch data across all systems

## Running the NATS docker images
* docker run --rm --name nats-main -p 4222:4222 -p 6222:6222 -p 8222:8222 nats:2.1.2-linux
* this is the default and lets you run a NATS server version 2.x (as of 12/2019)
* just runs in memory and no streaming (that is separate)

## What is required
* .NET Core 3.x
* running `dotnet add package NATS.Client` to add the package
* dotnet restore to pull in all required libraries
* The C# NATS client library available at https://github.com/nats-io/csharp-nats

## Making your local Docker image
* make build
* make latest

## creating the database users
* ~/mongodb/bin/mongo 'mongodb://root:myp2ssw0rd@localhost'
* use admin
* db.createUser({ user: "openrmfreports" , pwd: "openrmf1234!", roles: ["readWriteAnyDatabase"]});
* use openrmfreports

## connecting to the database collection straight
~/mongodb/bin/mongo 'mongodb://openrmfreports:openrmf1234!@localhost/openrmfreports?authSource=admin'
