// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using openrmf_report_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

using openrmf_report_api.Data;
using openrmf_report_api.Classes;

namespace openrmf_report_api.Controllers
{
    [Route("/")]
    public class ReportController : Controller
    {
	    private readonly IReportRepository _reportRepo;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportRepository reportRepo, ILogger<ReportController> logger)
        {
            _logger = logger;
            _reportRepo = reportRepo;
        }

        #region XLSX Formatting
        private DocumentFormat.OpenXml.Spreadsheet.Row MakeTitleRow(string title) {
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = 1 };
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "A1"};
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue(title.Trim());
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 2;
            return row;
        }
        private DocumentFormat.OpenXml.Spreadsheet.Row MakeXLSXInfoRow(string title, string value, uint rowindex) {
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowindex };
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "A" + rowindex.ToString()};
            row.InsertBefore(newCell, refCell);
            string cellValue = title + ": ";
            if (!String.IsNullOrEmpty(value))
                cellValue += value;
            else 
                cellValue += "N/A";
            newCell.CellValue = new CellValue(cellValue);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 3;
            return row;
        }
        private DocumentFormat.OpenXml.Spreadsheet.Row MakeChecklistHeaderRows(uint rowindex) {
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowindex };
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "A" + rowindex.ToString() };
            uint styleIndex = 3;

            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Vuln ID");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "B" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Severity");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "C" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Group Title");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "D" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Rule ID");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "E" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("STIG ID");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "F" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Rule Title");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "G" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Discussion");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "H" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("IA Controls");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "I" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Check Content");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "J" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Fix Text");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "K" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("False Positives");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "L" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("False Negatives");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "M" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Documentable");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "N" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Mitigations");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "O" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Potential Impact");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "P" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Third Party Tools");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "Q" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Mitigation Control");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "R" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Responsibility");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "S" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Severity Override Guidance");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "T" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Check Content Reference");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "U" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Classification");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "V" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("STIG");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "W" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Status");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "X" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Comments");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "Y" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Finding Details");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "Z" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Severity Override");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "AA" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Severity Override Justification");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "AB" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("CCI");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            return row;
        }
        private DocumentFormat.OpenXml.Spreadsheet.Row MakeNessusSummaryHeaderRows(uint rowindex, bool summaryOnly) {
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowindex };
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "A" + rowindex.ToString() };
            uint styleIndex = 3;

            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Plugin ID");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "B" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Plugin Name");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "C" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Family");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "D" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Severity");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "E" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Host Total");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "F" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Total");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            if (!summaryOnly) {
                // next column
                newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "G" + rowindex.ToString() };
                row.InsertBefore(newCell, refCell);
                newCell.CellValue = new CellValue("Host");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                newCell.StyleIndex = styleIndex;
            }
            return row;
        }
        private DocumentFormat.OpenXml.Spreadsheet.Row MakeTestPlanHeaderRows(uint rowindex, bool credentialed) {
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowindex };
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "A" + rowindex.ToString() };
            uint styleIndex = 3;

            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Host Name");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "B" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("IP Address");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "C" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Group Name");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "D" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Operating System");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "E" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("File Name");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "F" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("CAT I");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "G" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("CAT II");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "H" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("CAT III");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "I" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("CAT IV");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "J" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Total");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            if (credentialed) {
                // next column
                newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "K" + rowindex.ToString() };
                row.InsertBefore(newCell, refCell);
                newCell.CellValue = new CellValue("Credentialed");
                newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                newCell.StyleIndex = styleIndex;
            }
            return row;
        }
        private DocumentFormat.OpenXml.Spreadsheet.Row MakePOAMInfoRow(string colAlabel, string colBvalue, string colIlabel, string colJvalue, string colLlabel, string colMvalue, uint rowindex) {
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowindex };
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "B" + rowindex.ToString()};
            uint styleIndex = 16; // background color grey

            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue(colAlabel);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "C" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue(colBvalue);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 0;

            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "I" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue(colIlabel);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "J" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue(colJvalue);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 0;

            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "L" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue(colLlabel);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "M" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue(colMvalue);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 0;

            return row;
        }
        private DocumentFormat.OpenXml.Spreadsheet.Row MakePOAMHeaderRows(uint rowindex, bool credentialed) {
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowindex };
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "A" + rowindex.ToString() };
            uint styleIndex = 3;

            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "B" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Control Vulnerability Description");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "C" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Security Control Number");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "D" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Office/Org");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "E" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Security Checks");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "F" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Raw Severity Value");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "G" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Mitigations");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "H" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Severity Value");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "I" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Resources Required");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "J" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Scheduled Completion Date");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "K" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Milestone with Completion Dates");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "L" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Milestone with Completion Dates");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "M" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Source ID Control Vulnerability");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "N" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Status");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "O" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Comments");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;

            return row;
        }
        private DocumentFormat.OpenXml.Spreadsheet.Row MakeRARHeaderRows(uint rowindex, bool credentialed) {
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowindex };
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "A" + rowindex.ToString() };
            uint styleIndex = 3;

            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "B" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Non-Compliant Security Controls (16a)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "C" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Affected CCI (16a.1)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "D" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Source of Discovery (16a.2)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "E" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Vulnerability ID (16a.3)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "F" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Vulnerability Description (16b)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "G" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Devices Affected (16b.1)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "H" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Security Objectives (C-I-A) (16c)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "I" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Raw Test Result (16d)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "J" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Predisposing Condition(s) (16d.1)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "K" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Technical Mitigation(s) (16d.2)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "L" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Severity or Pervasiveness (VL-VH) (16d.3)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "M" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Relevance of Threat (VL-VH) (16e)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "N" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Threat Description (16e.1)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "O" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Likelihood (Cells 16d.3 & 16e) (VL-VH) (16f)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "P" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Impact (VL-VH) (16g)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "Q" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Impact Description (16h)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "R" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Risk (Cells 16f & 16g) (VL-VH) (16i)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "S" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Proposed Mitigations (From POA&M) (16j)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "T" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Residual Risk (After Proposed Mitigations) (16k)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "U" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Recommendations (16l)");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;

            return row;
        }
        
        private DocumentFormat.OpenXml.Spreadsheet.Row MakeDataRow(uint rowNumber, string cellReference, string value, uint styleIndex) {
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowNumber };
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = cellReference + rowNumber.ToString()};
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue(value);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            return row;
        }
        private DocumentFormat.OpenXml.Spreadsheet.Row MakeChecklistListingHeaderRows(uint rowindex) {
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;
            DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row() { RowIndex = rowindex };
            DocumentFormat.OpenXml.Spreadsheet.Cell newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "A" + rowindex.ToString() };
            uint styleIndex = 3;

            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("System");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "B" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Title");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "C" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Open");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 8;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "D" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Not a Finding");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 10;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "E" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Not Applicable");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 9;
            // next column
            newCell = new DocumentFormat.OpenXml.Spreadsheet.Cell() { CellReference = "F" + rowindex.ToString() };
            row.InsertBefore(newCell, refCell);
            newCell.CellValue = new CellValue("Not Reviewed");
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = 11;
            return row;
        }
        private uint GetVulnerabilityStatus(string status, string severity) {
            // open = 4, N/A = 5, NotAFinding = 6, Not Reviewed = 7
            if (status.ToLower() == "not_reviewed")
                return 7U;
            if (status.ToLower() == "open") { // need to know the category as well
                if (severity == "high")
                    return 4U;
                if (severity == "medium")
                    return 12U;
                return 13U; // catch all for Open CAT 3 Low
            }
            if (status.ToLower() == "not_applicable")
                return 5U;
            // catch all
            return 6U;
        }
        private uint GetPatchScanStatus(int severity) {
            // critical or high = 3 or 4, medium = 2, low = 1, informational = 0
            if (severity > 2)
                return 4U;
            if (severity == 2)
                return 12U;
            if (severity == 1)
                return 13U;
            // catch all informational
            return 7U;
        }
        #endregion

        /******************************************
        * Reports Specific API calls
        ******************************************/
        #region Report APIs

        /// <summary>
        /// GET The raw data of a Nessus file for the reports.
        /// </summary>
        /// <param name="systemGroupId">The ID of the system to use</param>
        /// <returns>
        /// HTTP Status showing it was found or that there is an error. And the raw data of 
        /// the Nessus ACAS Patch scan.
        /// </returns>
        /// <response code="200">Returns the count of records</response>
        /// <response code="400">If the item did not query correctly</response>
        /// <response code="404">If the ID passed in does not have a valid Nessus file</response>
        [HttpGet("system/{systemGroupId}/acaspatchdata")]
        [Authorize(Roles = "Administrator,Reader,Editor,Assessor")]
        public async Task<IActionResult> GetNessusPatchDataForReport(string systemGroupId)
        {
            if (!string.IsNullOrEmpty(systemGroupId)) {
                try {
                    _logger.LogInformation("Calling GetNessusPatchDataForReport({0})", systemGroupId);
                    IEnumerable<NessusPatchData> patchDataList;
                    patchDataList = await _reportRepo.GetPatchDataBySystem(systemGroupId);
                    if (patchDataList == null) {
                        _logger.LogWarning("GetNessusPatchDataForReport({0}) has an invalid system record or no Nessus data", systemGroupId);
                        return NotFound();
                    }
                    _logger.LogInformation("Called GetNessusPatchDataForReport({0}) successfully", systemGroupId);
                    return Ok(patchDataList);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "GetNessusPatchDataForReport() Error getting data for the Nessus Data report for system {0}", systemGroupId);
                    return BadRequest();
                }
            }
            else {
                _logger.LogWarning("Called GetNessusPatchDataForReport() with no system ID");
                return BadRequest(); // no systemGroupId entered
            }
        }
        
        /// <summary>
        /// GET The list of hostnames and vulnerability data from a passed in system id and vuln id
        /// </summary>
        /// <param name="systemGroupId">The ID of the system to use</param>
        /// <param name="vulnid">The ID of the vulnerability to use</param>
        /// <returns>
        /// HTTP Status showing data was found or that there is an error. And the listing of data 
        /// for the vulnerability report
        /// </returns>
        /// <response code="200">Returns the listing of vulnerability report records</response>
        /// <response code="400">If the item did not query correctly</response>
        /// <response code="404">If the ID passed is not a valid system</response>
        [HttpGet("system/{systemGroupId}/vulnid/{vulnid}")]
        [Authorize(Roles = "Administrator,Reader,Editor,Assessor")]
        public async Task<IActionResult> GetSystemByVulnerabilityForReport(string systemGroupId, string vulnid)
        {
            if (!string.IsNullOrEmpty(systemGroupId) || !string.IsNullOrEmpty(vulnid)) {
                try {
                    _logger.LogInformation("Calling GetSystemByVulnerabilityForReport(system: {0}, vulnid: {1})", systemGroupId, vulnid);
                    IEnumerable<VulnerabilityReport> vulnerabilities =  await _reportRepo.GetChecklistVulnerabilityData(systemGroupId, vulnid);
                    if (vulnerabilities == null) {
                        _logger.LogWarning("Calling GetSystemByVulnerabilityForReport(system: {0}, vulnid: {1}) returned no checklist vulnerabilities", systemGroupId, vulnid  );
                        return NotFound();
                    }
                    _logger.LogInformation("Called GetSystemByVulnerabilityForReport(system: {0}, vulnid: {1}) successfully", systemGroupId, vulnid);
                    return Ok(vulnerabilities);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "GetSystemByVulnerabilityForReport() Error listing all checklist vulnerabilities for system: {0}, vulnid: {1}", systemGroupId, vulnid);
                    return BadRequest();
                }
            }
            else {
                _logger.LogWarning("Called GetSystemByVulnerabilityForReport(systemId, vulnId) with no system Id or vulnerability Id");
                return BadRequest(); // no systemGroupId or vulnId entered
            }
        }

        /// <summary>
        /// GET The list of hostnames and vulnerability data from a passed in system id and
        /// status and severity checks
        /// </summary>
        /// <param name="systemGroupId">The ID of the system to use</param>
        /// <param name="naf">Optional: Include Not a Finding items</param>
        /// <param name="open">IOptional: nclude Open Items</param>
        /// <param name="na">Optional: Include Not Applicable Items</param>
        /// <param name="nr">Optional: Include Not Reviewed Items</param>
        /// <param name="cat1">Optional: Include Category 1 (High) Items</param>
        /// <param name="cat2">Optional: Include Category 2 (Medium) Items</param>
        /// <param name="cat3">Optional: Include Category 3 (Low) Items</param>
        /// <returns>
        /// HTTP Status showing data was found or that there is an error. And the listing of data 
        /// for the vulnerability report
        /// </returns>
        /// <response code="200">Returns the listing of vulnerability report records</response>
        /// <response code="400">If the item did not query correctly</response>
        /// <response code="404">If the ID passed is not a valid system</response>
        [HttpGet("system/{systemGroupId}")]
        [Authorize(Roles = "Administrator,Reader,Editor,Assessor")]
        public async Task<IActionResult> GetSystemByVulnerabilityByStatusSeverityForReport(string systemGroupId, bool naf = true, bool open = true, bool na = true, 
            bool nr = true, bool cat1 = true, bool cat2 = true, bool cat3 = true)
        {

            try {
                _logger.LogInformation("Calling GetSystemByVulnerabilityByStatusSeverityForReport(system: {0})", systemGroupId);
                // fill the datatable response required
                // get the categories together
                List<string> categories = new List<string>();
                if (cat1) categories.Add("high");
                if (cat2) categories.Add("medium");
                if (cat3) categories.Add("low");
                // get the statuses together
                List<string> statuses = new List<string>();
                if (open) statuses.Add("Open");
                if (na) statuses.Add("Not_Applicable");
                if (naf) statuses.Add("NotAFinding");
                if (nr) statuses.Add("Not_Reviewed");
                
                List<VulnerabilityReport> vulnerabilities =  await _reportRepo.GetSystemVulnerabilityData(systemGroupId, categories, statuses);
                if (vulnerabilities == null) {
                    _logger.LogWarning("Calling GetSystemByVulnerabilityByStatusSeverityForReport(system: {0}) returned no checklist vulnerabilities", systemGroupId);
                    return Ok(new List<VulnerabilityReport>());
                }
                _logger.LogInformation("Called GetSystemByVulnerabilityByStatusSeverityForReport(system: {0}) successfully", systemGroupId);
                return Ok(vulnerabilities);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "GetSystemByVulnerabilityByStatusSeverityForReport() Error listing all checklist vulnerabilities for system: {0}", systemGroupId);
                return BadRequest();
            }
        }

        /// <summary>
        /// GET The list of hostnames and vulnerability data from a passed in system id that
        /// has override settings set
        /// </summary>
        /// <param name="systemGroupId">The ID of the system to use</param>
        /// <returns>
        /// HTTP Status showing data was found or that there is an error. And the listing of data 
        /// for the vulnerability report
        /// </returns>
        /// <response code="200">Returns the listing of vulnerability report records</response>
        /// <response code="400">If the item did not query correctly</response>
        /// <response code="404">If the ID passed is not a valid system</response>
        [HttpGet("system/{systemGroupId}/override")]
        [Authorize(Roles = "Administrator,Reader,Editor,Assessor")]
        public async Task<IActionResult> GetSystemByVulnerabilityByOverrideReport(string systemGroupId)
        {

            try {
                _logger.LogInformation("Calling GetSystemByVulnerabilityByOverrideReport(system: {0})", systemGroupId);
               
                IEnumerable<VulnerabilityReport> vulnerabilities =  await _reportRepo.GetChecklistVulnerabilityOverrideData(systemGroupId);
                if (vulnerabilities == null) {
                    _logger.LogWarning("Calling GetSystemByVulnerabilityByOverrideReport(system: {0}) returned no checklist vulnerabilities with override", systemGroupId);
                    return Ok(new List<VulnerabilityReport>());
                }
                _logger.LogInformation("Called GetSystemByVulnerabilityByOverrideReport(system: {0}) successfully", systemGroupId);
                return Ok(vulnerabilities);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "GetSystemByVulnerabilityByOverrideReport() Error listing all checklist vulnerabilities with override for system package: {0}", systemGroupId);
                return BadRequest();
            }
        }

        [HttpGet("system/{systemGroupId}/missingdata")]
        [Authorize(Roles = "Administrator,Reader,Editor,Assessor")]
        public async Task<IActionResult> GetSystemByVulnerabilityByMissingData(string systemGroupId)
        {

            try {
                _logger.LogInformation("Calling GetSystemByVulnerabilityByMissingData(system: {0})", systemGroupId);
               
                IEnumerable<VulnerabilityReport> vulnerabilities =  await _reportRepo.GetChecklistVulnerabilityMissingKeyData(systemGroupId);
                if (vulnerabilities == null) {
                    _logger.LogWarning("Calling GetSystemByVulnerabilityByOverriGetSystemByVulnerabilityByMissingDatadeReport(system: {0}) returned no checklist vulnerabilities with override", systemGroupId);
                    return Ok(new List<VulnerabilityReport>());
                }
                _logger.LogInformation("Called GetSystemByVulnerabilityByMissingData(system: {0}) successfully", systemGroupId);
                return Ok(vulnerabilities);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "GetSystemByVulnerabilityByMissingData() Error listing all checklist vulnerabilities with override for system package: {0}", systemGroupId);
                return BadRequest();
            }
        }

        

        #endregion
    }
}
