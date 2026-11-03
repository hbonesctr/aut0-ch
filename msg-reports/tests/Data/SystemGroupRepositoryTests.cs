using Xunit;
using Moq;
using openrmf_msg_report.Data;
using openrmf_msg_report.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.Data
{
    public class SystemGroupRepositoryTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static SystemGroup BuildSystemGroup(string id = "sys123", string title = "Test System")
        {
            return new SystemGroup
            {
                title = title,
                description = "A test system group",
                numberOfChecklists = 3,
                created = DateTime.Now,
                createdBy = Guid.NewGuid()
            };
        }

        // ── GetSystemGroup – Pass tests ────────────────────────────────────────

        [Fact]
        public async Task Test_GetSystemGroup_ReturnsSystemGroup_WhenIdExists()
        {
            var expected = BuildSystemGroup();
            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetSystemGroup("sys123"))
                    .ReturnsAsync(expected);

            var result = await mockRepo.Object.GetSystemGroup("sys123");

            Assert.NotNull(result);
            Assert.Equal(expected.title, result.title);
            Assert.Equal(expected.numberOfChecklists, result.numberOfChecklists);
        }

        [Fact]
        public async Task Test_GetSystemGroup_CallsRepositoryOnce()
        {
            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetSystemGroup(It.IsAny<string>()))
                    .ReturnsAsync(BuildSystemGroup());

            await mockRepo.Object.GetSystemGroup("sys123");

            mockRepo.Verify(r => r.GetSystemGroup("sys123"), Times.Once);
        }

        [Fact]
        public async Task Test_GetSystemGroup_ReturnsCorrectTitle()
        {
            var sg = BuildSystemGroup(title: "My Production System");
            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetSystemGroup("prod-id"))
                    .ReturnsAsync(sg);

            var result = await mockRepo.Object.GetSystemGroup("prod-id");

            Assert.Equal("My Production System", result.title);
        }

        [Fact]
        public async Task Test_GetSystemGroup_WithNessusFile_HasRawNessusData()
        {
            var sg = BuildSystemGroup();
            sg.rawNessusFile = "<NessusClientData_v2/>";
            sg.nessusFilename = "scan.nessus";

            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetSystemGroup("sys-with-nessus"))
                    .ReturnsAsync(sg);

            var result = await mockRepo.Object.GetSystemGroup("sys-with-nessus");

            Assert.NotEmpty(result.rawNessusFile);
            Assert.Equal("scan.nessus", result.nessusFilename);
        }

        // ── GetSystemGroup – Fail / negative tests ─────────────────────────────

        [Fact]
        public async Task Test_GetSystemGroup_ReturnsNull_WhenIdDoesNotExist()
        {
            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetSystemGroup("nonexistent"))
                    .ReturnsAsync((SystemGroup)null);

            var result = await mockRepo.Object.GetSystemGroup("nonexistent");

            Assert.Null(result);
        }

        [Fact]
        public async Task Test_GetSystemGroup_WrongId_DoesNotReturnGroup()
        {
            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetSystemGroup("correct-id")).ReturnsAsync(BuildSystemGroup());
            mockRepo.Setup(r => r.GetSystemGroup("wrong-id")).ReturnsAsync((SystemGroup)null);

            var result = await mockRepo.Object.GetSystemGroup("wrong-id");

            Assert.Null(result);
        }

        // ── GetAllSystemGroups – Pass tests ────────────────────────────────────

        [Fact]
        public async Task Test_GetAllSystemGroups_ReturnsListOfGroups()
        {
            var groups = new List<SystemGroup>
            {
                BuildSystemGroup("1", "System A"),
                BuildSystemGroup("2", "System B"),
                BuildSystemGroup("3", "System C")
            };

            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetAllSystemGroups()).ReturnsAsync(groups);

            var result = await mockRepo.Object.GetAllSystemGroups();

            Assert.NotNull(result);
            Assert.Equal(3, ((List<SystemGroup>)result).Count);
        }

        [Fact]
        public async Task Test_GetAllSystemGroups_CallsRepositoryOnce()
        {
            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetAllSystemGroups())
                    .ReturnsAsync(new List<SystemGroup>());

            await mockRepo.Object.GetAllSystemGroups();

            mockRepo.Verify(r => r.GetAllSystemGroups(), Times.Once);
        }

        [Fact]
        public async Task Test_GetAllSystemGroups_EachGroup_HasTitle()
        {
            var groups = new List<SystemGroup>
            {
                BuildSystemGroup("1", "Alpha"),
                BuildSystemGroup("2", "Beta")
            };

            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetAllSystemGroups()).ReturnsAsync(groups);

            var result = await mockRepo.Object.GetAllSystemGroups();

            foreach (var sg in result)
                Assert.NotNull(sg.title);
        }

        // ── GetAllSystemGroups – Fail / negative tests ─────────────────────────

        [Fact]
        public async Task Test_GetAllSystemGroups_ReturnsEmptyList_WhenNoneExist()
        {
            var mockRepo = new Mock<ISystemGroupRepository>();
            mockRepo.Setup(r => r.GetAllSystemGroups())
                    .ReturnsAsync(new List<SystemGroup>());

            var result = await mockRepo.Object.GetAllSystemGroups();

            Assert.Empty(result);
        }
    }
}
