using Xunit;
using openrmf_report_api.Controllers;
using openrmf_report_api.Data;
using openrmf_report_api.Models;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.Controllers
{
    public class ReportControllerTests
    {
        private readonly Mock<ILogger<ReportController>> _mockLogger;
        private readonly Mock<IReportRepository> _mockReportRepo;
        private readonly ReportController _controller;

        public ReportControllerTests()
        {
            _mockLogger = new Mock<ILogger<ReportController>>();
            _mockReportRepo = new Mock<IReportRepository>();
            _controller = new ReportController(_mockReportRepo.Object, _mockLogger.Object);
        }

        // ---- ReportController Instantiation ----

        [Fact]
        public void Test_ReportControllerIsNotNull()
        {
            Assert.NotNull(_controller);
        }

        // ---- GetNessusPatchDataForReport ----

        [Fact]
        public async Task Test_GetNessusPatchDataForReport_ReturnsOkWithValidId()
        {
            var patchData = new List<NessusPatchData>
            {
                new NessusPatchData { systemGroupId = "sys001", hostname = "host1", severity = 3 }
            };
            _mockReportRepo.Setup(r => r.GetPatchDataBySystem("sys001"))
                .ReturnsAsync(patchData);

            var result = await _controller.GetNessusPatchDataForReport("sys001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Test_GetNessusPatchDataForReport_ReturnsNotFoundWhenNull()
        {
            _mockReportRepo.Setup(r => r.GetPatchDataBySystem("missing"))
                .ReturnsAsync((IEnumerable<NessusPatchData>)null);

            var result = await _controller.GetNessusPatchDataForReport("missing");
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Test_GetNessusPatchDataForReport_ReturnsBadRequestForEmptyId()
        {
            var result = await _controller.GetNessusPatchDataForReport("");
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Test_GetNessusPatchDataForReport_ReturnsBadRequestOnException()
        {
            _mockReportRepo.Setup(r => r.GetPatchDataBySystem("sys001"))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetNessusPatchDataForReport("sys001");
            Assert.IsType<BadRequestResult>(result);
        }

        // ---- GetSystemByVulnerabilityForReport ----

        [Fact]
        public async Task Test_GetSystemByVulnerabilityForReport_ReturnsOkWithValidIds()
        {
            var vulns = new List<VulnerabilityReport>
            {
                new VulnerabilityReport { vulnid = "V-12345", hostname = "host1", severity = "high" }
            };
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityData("sys001", "V-12345"))
                .ReturnsAsync(vulns);

            var result = await _controller.GetSystemByVulnerabilityForReport("sys001", "V-12345");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Test_GetSystemByVulnerabilityForReport_ReturnsNotFoundWhenNull()
        {
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityData("sys001", "V-99999"))
                .ReturnsAsync((IEnumerable<VulnerabilityReport>)null);

            var result = await _controller.GetSystemByVulnerabilityForReport("sys001", "V-99999");
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Test_GetSystemByVulnerabilityForReport_ReturnsBadRequestOnException()
        {
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityData("sys001", "V-12345"))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetSystemByVulnerabilityForReport("sys001", "V-12345");
            Assert.IsType<BadRequestResult>(result);
        }

        // ---- GetSystemByVulnerabilityByStatusSeverityForReport ----

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByStatusSeverity_ReturnsOkWithData()
        {
            var vulns = new List<VulnerabilityReport>
            {
                new VulnerabilityReport { vulnid = "V-00001", severity = "high", status = "Open" }
            };
            _mockReportRepo.Setup(r => r.GetSystemVulnerabilityData(
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<string>>()))
                .ReturnsAsync(vulns);

            var result = await _controller.GetSystemByVulnerabilityByStatusSeverityForReport("sys001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByStatusSeverity_ReturnsOkEmptyListWhenNull()
        {
            _mockReportRepo.Setup(r => r.GetSystemVulnerabilityData(
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<string>>()))
                .ReturnsAsync((List<VulnerabilityReport>)null);

            var result = await _controller.GetSystemByVulnerabilityByStatusSeverityForReport("sys001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsType<List<VulnerabilityReport>>(okResult.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByStatusSeverity_ReturnsBadRequestOnException()
        {
            _mockReportRepo.Setup(r => r.GetSystemVulnerabilityData(
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<string>>()))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetSystemByVulnerabilityByStatusSeverityForReport("sys001");
            Assert.IsType<BadRequestResult>(result);
        }

        // ---- GetSystemByVulnerabilityByOverrideReport ----

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByOverrideReport_ReturnsOkWithData()
        {
            var vulns = new List<VulnerabilityReport>
            {
                new VulnerabilityReport { vulnid = "V-00002", severityOverride = "medium" }
            };
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityOverrideData("sys001"))
                .ReturnsAsync(vulns);

            var result = await _controller.GetSystemByVulnerabilityByOverrideReport("sys001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByOverrideReport_ReturnsOkEmptyListWhenNull()
        {
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityOverrideData("sys001"))
                .ReturnsAsync((IEnumerable<VulnerabilityReport>)null);

            var result = await _controller.GetSystemByVulnerabilityByOverrideReport("sys001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsType<List<VulnerabilityReport>>(okResult.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByOverrideReport_ReturnsBadRequestOnException()
        {
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityOverrideData("sys001"))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetSystemByVulnerabilityByOverrideReport("sys001");
            Assert.IsType<BadRequestResult>(result);
        }

        // ---- GetSystemByVulnerabilityByMissingData ----

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByMissingData_ReturnsOkWithData()
        {
            var vulns = new List<VulnerabilityReport>
            {
                new VulnerabilityReport { vulnid = "V-00003", details = "" }
            };
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityMissingKeyData("sys001"))
                .ReturnsAsync(vulns);

            var result = await _controller.GetSystemByVulnerabilityByMissingData("sys001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByMissingData_ReturnsOkEmptyListWhenNull()
        {
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityMissingKeyData("sys001"))
                .ReturnsAsync((IEnumerable<VulnerabilityReport>)null);

            var result = await _controller.GetSystemByVulnerabilityByMissingData("sys001");
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsType<List<VulnerabilityReport>>(okResult.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task Test_GetSystemByVulnerabilityByMissingData_ReturnsBadRequestOnException()
        {
            _mockReportRepo.Setup(r => r.GetChecklistVulnerabilityMissingKeyData("sys001"))
                .ThrowsAsync(new Exception("DB error"));

            var result = await _controller.GetSystemByVulnerabilityByMissingData("sys001");
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
