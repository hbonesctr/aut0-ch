using Xunit;
using Moq;
using openrmf_msg_report.Data;
using openrmf_msg_report.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.Data
{
    public class ReportRepositoryTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static NessusPatchData BuildPatchData(string systemGroupId = "sys123")
        {
            return new NessusPatchData
            {
                systemGroupId = systemGroupId,
                hostname = "SCANNER01",
                reportName = "TestReport",
                operatingSystem = "Windows Server 2019",
                systemType = "general-purpose",
                ipAddress = "xxx.xxx.1.1",
                credentialed = true,
                pluginId = "12345",
                pluginName = "Test Plugin",
                family = "General",
                severity = 2,
                created = DateTime.Now
            };
        }

        private static VulnerabilityReport BuildVulnReport(string systemGroupId = "sys123", string artifactId = "art456")
        {
            return new VulnerabilityReport
            {
                systemGroupId = systemGroupId,
                artifactId = artifactId,
                vulnid = "V-220697",
                hostname = "TESTHOST",
                checklistType = "WIN 10",
                checklistRelease = "R23",
                checklistVersion = "1",
                severity = "high",
                status = "Open",
                created = DateTime.Now,
                createdBy = Guid.NewGuid()
            };
        }

        // ── GetAllPatchScanDataBySystemGroup – Pass tests ──────────────────────

        [Fact]
        public async Task Test_GetAllPatchScanData_ReturnsListForSystemGroup()
        {
            var patchList = new List<NessusPatchData>
            {
                BuildPatchData("sys123"),
                BuildPatchData("sys123")
            };

            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetAllPatchScanDataBySystemGroup("sys123"))
                    .ReturnsAsync(patchList);

            var result = await mockRepo.Object.GetAllPatchScanDataBySystemGroup("sys123");

            Assert.NotNull(result);
            Assert.Equal(2, ((List<NessusPatchData>)result).Count);
        }

        [Fact]
        public async Task Test_GetAllPatchScanData_CallsRepositoryOnce()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetAllPatchScanDataBySystemGroup(It.IsAny<string>()))
                    .ReturnsAsync(new List<NessusPatchData>());

            await mockRepo.Object.GetAllPatchScanDataBySystemGroup("sys123");

            mockRepo.Verify(r => r.GetAllPatchScanDataBySystemGroup("sys123"), Times.Once);
        }

        [Fact]
        public async Task Test_GetAllPatchScanData_EachItem_HasCorrectSystemGroupId()
        {
            var patchList = new List<NessusPatchData> { BuildPatchData("sys-xyz"), BuildPatchData("sys-xyz") };
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetAllPatchScanDataBySystemGroup("sys-xyz")).ReturnsAsync(patchList);

            var result = await mockRepo.Object.GetAllPatchScanDataBySystemGroup("sys-xyz");

            foreach (var item in result)
                Assert.Equal("sys-xyz", item.systemGroupId);
        }

        // ── GetAllPatchScanDataBySystemGroup – Fail / negative tests ───────────

        [Fact]
        public async Task Test_GetAllPatchScanData_ReturnsEmpty_WhenNoDataExists()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetAllPatchScanDataBySystemGroup("empty-sys"))
                    .ReturnsAsync(new List<NessusPatchData>());

            var result = await mockRepo.Object.GetAllPatchScanDataBySystemGroup("empty-sys");

            Assert.Empty(result);
        }

        // ── AddPatchScanData – Pass tests ──────────────────────────────────────

        [Fact]
        public async Task Test_AddPatchScanData_ReturnsAddedRecord()
        {
            var patch = BuildPatchData();
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.AddPatchScanData(patch)).ReturnsAsync(patch);

            var result = await mockRepo.Object.AddPatchScanData(patch);

            Assert.NotNull(result);
            Assert.Equal(patch.pluginId, result.pluginId);
            Assert.Equal(patch.systemGroupId, result.systemGroupId);
        }

        [Fact]
        public async Task Test_AddPatchScanData_CallsRepositoryOnce()
        {
            var patch = BuildPatchData();
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.AddPatchScanData(It.IsAny<NessusPatchData>())).ReturnsAsync(patch);

            await mockRepo.Object.AddPatchScanData(patch);

            mockRepo.Verify(r => r.AddPatchScanData(patch), Times.Once);
        }

        // ── AddPatchScanDataBulk – Pass tests ──────────────────────────────────

        [Fact]
        public async Task Test_AddPatchScanDataBulk_ReturnsAllItems()
        {
            var items = new List<NessusPatchData>
            {
                BuildPatchData("sys1"),
                BuildPatchData("sys1"),
                BuildPatchData("sys1")
            };

            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.AddPatchScanDataBulk(items)).ReturnsAsync(items);

            var result = await mockRepo.Object.AddPatchScanDataBulk(items);

            Assert.Equal(3, result.Count);
        }

        // ── DeleteAllSystemData – Pass tests ───────────────────────────────────

        [Fact]
        public async Task Test_DeleteAllSystemData_ReturnsTrue_WhenSuccessful()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.DeleteAllSystemData("sys123")).ReturnsAsync(true);

            var result = await mockRepo.Object.DeleteAllSystemData("sys123");

            Assert.True(result);
        }

        // ── DeleteAllSystemData – Fail / negative tests ────────────────────────

        [Fact]
        public async Task Test_DeleteAllSystemData_ReturnsFalse_WhenNothingToDelete()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.DeleteAllSystemData("no-data-sys")).ReturnsAsync(false);

            var result = await mockRepo.Object.DeleteAllSystemData("no-data-sys");

            Assert.False(result);
        }

        // ── DeletePatchScanDataBySystemGroup – Pass/Fail tests ─────────────────

        [Fact]
        public async Task Test_DeletePatchScanData_ReturnsTrue_WhenSuccessful()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.DeletePatchScanDataBySystemGroup("sys123")).ReturnsAsync(true);

            var result = await mockRepo.Object.DeletePatchScanDataBySystemGroup("sys123");

            Assert.True(result);
        }

        [Fact]
        public async Task Test_DeletePatchScanData_ReturnsFalse_WhenNothingDeleted()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.DeletePatchScanDataBySystemGroup("empty-sys")).ReturnsAsync(false);

            var result = await mockRepo.Object.DeletePatchScanDataBySystemGroup("empty-sys");

            Assert.False(result);
        }

        // ── VulnerabilityReport – Pass tests ───────────────────────────────────

        [Fact]
        public async Task Test_AddChecklistVulnerabilityData_ReturnsRecord()
        {
            var vuln = BuildVulnReport();
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.AddChecklistVulnerabilityData(vuln)).ReturnsAsync(vuln);

            var result = await mockRepo.Object.AddChecklistVulnerabilityData(vuln);

            Assert.NotNull(result);
            Assert.Equal(vuln.vulnid, result.vulnid);
            Assert.Equal(vuln.systemGroupId, result.systemGroupId);
        }

        [Fact]
        public async Task Test_AddChecklistVulnerabilityDataBulk_ReturnsAllRecords()
        {
            var items = new List<VulnerabilityReport>
            {
                BuildVulnReport("sys1", "art1"),
                BuildVulnReport("sys1", "art2")
            };

            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.AddChecklistVulnerabilityDataBulk(items)).ReturnsAsync(items);

            var result = await mockRepo.Object.AddChecklistVulnerabilityDataBulk(items);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Test_UpdateChecklistVulnerabilityData_ReturnsTrue_WhenSuccessful()
        {
            var vuln = BuildVulnReport();
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.UpdateChecklistVulnerabilityData(vuln)).ReturnsAsync(true);

            var result = await mockRepo.Object.UpdateChecklistVulnerabilityData(vuln);

            Assert.True(result);
        }

        [Fact]
        public async Task Test_DeleteChecklistVulnerabilityData_ReturnsTrue_WhenSuccessful()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.DeleteChecklistVulnerabilityData("art456")).ReturnsAsync(true);

            var result = await mockRepo.Object.DeleteChecklistVulnerabilityData("art456");

            Assert.True(result);
        }

        [Fact]
        public async Task Test_DeleteChecklistVulnerabilityDataBySystemGroup_ReturnsTrue_WhenSuccessful()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.DeleteChecklistVulnerabilityDataBySystemGroup("sys123")).ReturnsAsync(true);

            var result = await mockRepo.Object.DeleteChecklistVulnerabilityDataBySystemGroup("sys123");

            Assert.True(result);
        }

        [Fact]
        public async Task Test_GetChecklistVulnerabilityData_ReturnsRecord_WhenExists()
        {
            var vuln = BuildVulnReport("sys123", "art456");
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetChecklistVulnerabilityData("sys123", "art456", "V-220697"))
                    .ReturnsAsync(vuln);

            var result = await mockRepo.Object.GetChecklistVulnerabilityData("sys123", "art456", "V-220697");

            Assert.NotNull(result);
            Assert.Equal("V-220697", result.vulnid);
            Assert.Equal("sys123", result.systemGroupId);
        }

        [Fact]
        public async Task Test_FindChecklistVulnerabilityData_ReturnsMultipleRecords()
        {
            var records = new List<VulnerabilityReport>
            {
                BuildVulnReport("sys1", "art1"),
                BuildVulnReport("sys1", "art2")
            };

            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.FindChecklistVulnerabilityData("sys1", "V-220697"))
                    .ReturnsAsync(records);

            var result = await mockRepo.Object.FindChecklistVulnerabilityData("sys1", "V-220697");

            Assert.NotNull(result);
            Assert.Equal(2, ((List<VulnerabilityReport>)result).Count);
        }

        // ── VulnerabilityReport – Fail / negative tests ────────────────────────

        [Fact]
        public async Task Test_GetChecklistVulnerabilityData_ReturnsNull_WhenNotFound()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetChecklistVulnerabilityData("bad-sys", "bad-art", "V-999"))
                    .ReturnsAsync((VulnerabilityReport)null);

            var result = await mockRepo.Object.GetChecklistVulnerabilityData("bad-sys", "bad-art", "V-999");

            Assert.Null(result);
        }

        [Fact]
        public async Task Test_UpdateChecklistVulnerabilityData_ReturnsFalse_WhenRecordNotFound()
        {
            var vuln = BuildVulnReport("missing-sys", "missing-art");
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.UpdateChecklistVulnerabilityData(vuln)).ReturnsAsync(false);

            var result = await mockRepo.Object.UpdateChecklistVulnerabilityData(vuln);

            Assert.False(result);
        }

        [Fact]
        public async Task Test_DeleteChecklistVulnerabilityData_ReturnsFalse_WhenNotFound()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.DeleteChecklistVulnerabilityData("nonexistent-art")).ReturnsAsync(false);

            var result = await mockRepo.Object.DeleteChecklistVulnerabilityData("nonexistent-art");

            Assert.False(result);
        }

        [Fact]
        public async Task Test_FindChecklistVulnerabilityData_ReturnsEmpty_WhenNoneFound()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.FindChecklistVulnerabilityData("sys-no-data", "V-000000"))
                    .ReturnsAsync(new List<VulnerabilityReport>());

            var result = await mockRepo.Object.FindChecklistVulnerabilityData("sys-no-data", "V-000000");

            Assert.Empty(result);
        }
    }
}
