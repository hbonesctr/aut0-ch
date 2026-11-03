![.NET Core Build and Test](https://github.com/Cingulara/openrmf-api-reports/workflows/.NET%20Core%20Build%20and%20Test/badge.svg)

# openrmf-api-report
This is the OpenRMF Reports API for running reports with data formatted for quick querying.

* GET to /system/{systemGroupId}/acaspatchdata to get a NESSUS ACAS Patch data report
* GET to /system/{systemGroupId}/vulnid/{vulnid} to get a list of Vunlerabilities based on the VULN ID passed
* POST to /reloaddata to reload report data from checklists and Nessus ACAS scan file data
* /swagger/ gives you the API structure.

## NATS Messaging calls made
* openrmf.report.refresh.nessuspatchdata to reload Nessus data across all systems
* openrmf.report.refresh.vulnerabilitydata to reload all individual vulnerability data per checklists across all systems

## Making your local Docker image
* make build
* make latest

## creating the user
* ~/mongodb/bin/mongo 'mongodb://root:myp2ssw0rd@localhost'
* use admin
* db.createUser({ user: "openrmfreport" , pwd: "openrmf1234!", roles: ["readWriteAnyDatabase"]});
* use openrmfreport

## connecting to the database collection straight
~/mongodb/bin/mongo 'mongodb://openrmfreport:openrmf1234!@localhost/openrmfreport?authSource=admin'

## Using Jaeger

The Jaeger Client is https://github.com/jaegertracing/jaeger-client-csharp. We use defaults but you can specify ENV for configuration.