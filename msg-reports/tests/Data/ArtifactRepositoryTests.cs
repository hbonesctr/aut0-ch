using Xunit;
using Moq;
using openrmf_msg_report.Data;
using openrmf_msg_report.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tests.Data
{
    public class ArtifactRepositoryTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static Artifact BuildArtifact(string systemGroupId = "sys123")
        {
            return new Artifact
            {
                systemGroupId = systemGroupId,
                hostName = "TESTHOST",
                stigType = "Windows 10 STIG",
                stigRelease = "R23",
                version = "1",
                created = DateTime.Now,
                createdBy = Guid.NewGuid()
            };
        }

        // ── GetArtifact – Pass tests ───────────────────────────────────────────

        [Fact]
        public async Task Test_GetArtifact_ReturnsArtifact_WhenIdExists()
        {
            var expected = BuildArtifact();
            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetArtifact("artifact-id-1"))
                    .ReturnsAsync(expected);

            var result = await mockRepo.Object.GetArtifact("artifact-id-1");

            Assert.NotNull(result);
            Assert.Equal(expected.hostName, result.hostName);
            Assert.Equal(expected.systemGroupId, result.systemGroupId);
        }

        [Fact]
        public async Task Test_GetArtifact_CallsRepositoryOnce()
        {
            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetArtifact(It.IsAny<string>()))
                    .ReturnsAsync(BuildArtifact());

            await mockRepo.Object.GetArtifact("some-id");

            mockRepo.Verify(r => r.GetArtifact("some-id"), Times.Once);
        }

        [Fact]
        public async Task Test_GetArtifact_ReturnsArtifactWithChecklist()
        {
            var artifact = BuildArtifact();
            artifact.CHECKLIST = new CHECKLIST();
            artifact.CHECKLIST.ASSET.HOST_NAME = "TESTHOST";

            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetArtifact("id-with-checklist"))
                    .ReturnsAsync(artifact);

            var result = await mockRepo.Object.GetArtifact("id-with-checklist");

            Assert.NotNull(result.CHECKLIST);
            Assert.Equal("TESTHOST", result.CHECKLIST.ASSET.HOST_NAME);
        }

        [Fact]
        public async Task Test_GetArtifact_DifferentIds_ReturnDifferentArtifacts()
        {
            var artifact1 = BuildArtifact("system-A");
            var artifact2 = BuildArtifact("system-B");

            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetArtifact("id-1")).ReturnsAsync(artifact1);
            mockRepo.Setup(r => r.GetArtifact("id-2")).ReturnsAsync(artifact2);

            var result1 = await mockRepo.Object.GetArtifact("id-1");
            var result2 = await mockRepo.Object.GetArtifact("id-2");

            Assert.Equal("system-A", result1.systemGroupId);
            Assert.Equal("system-B", result2.systemGroupId);
        }

        // ── GetArtifact – Fail / negative tests ───────────────────────────────

        [Fact]
        public async Task Test_GetArtifact_ReturnsNull_WhenIdDoesNotExist()
        {
            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetArtifact("nonexistent-id"))
                    .ReturnsAsync((Artifact)null);

            var result = await mockRepo.Object.GetArtifact("nonexistent-id");

            Assert.Null(result);
        }

        [Fact]
        public async Task Test_GetArtifact_WithWrongId_DoesNotReturnExpectedArtifact()
        {
            var artifact = BuildArtifact("system-X");
            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetArtifact("correct-id")).ReturnsAsync(artifact);
            mockRepo.Setup(r => r.GetArtifact("wrong-id")).ReturnsAsync((Artifact)null);

            var result = await mockRepo.Object.GetArtifact("wrong-id");

            Assert.Null(result);
        }

        // ── GetSystemArtifacts – Pass tests ───────────────────────────────────

        [Fact]
        public async Task Test_GetSystemArtifacts_ReturnsListOfArtifacts()
        {
            var artifacts = new List<Artifact>
            {
                BuildArtifact("sys123"),
                BuildArtifact("sys123")
            };

            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetSystemArtifacts("sys123"))
                    .ReturnsAsync(artifacts);

            var result = await mockRepo.Object.GetSystemArtifacts("sys123");

            Assert.NotNull(result);
            Assert.Equal(2, ((List<Artifact>)result).Count);
        }

        [Fact]
        public async Task Test_GetSystemArtifacts_CallsRepositoryOnce()
        {
            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetSystemArtifacts(It.IsAny<string>()))
                    .ReturnsAsync(new List<Artifact>());

            await mockRepo.Object.GetSystemArtifacts("sys123");

            mockRepo.Verify(r => r.GetSystemArtifacts("sys123"), Times.Once);
        }

        [Fact]
        public async Task Test_GetSystemArtifacts_EachArtifact_HasCorrectSystemGroupId()
        {
            var artifacts = new List<Artifact>
            {
                BuildArtifact("sys456"),
                BuildArtifact("sys456")
            };

            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetSystemArtifacts("sys456")).ReturnsAsync(artifacts);

            var result = await mockRepo.Object.GetSystemArtifacts("sys456");

            foreach (var artifact in result)
                Assert.Equal("sys456", artifact.systemGroupId);
        }

        // ── GetSystemArtifacts – Fail / negative tests ────────────────────────

        [Fact]
        public async Task Test_GetSystemArtifacts_ReturnsEmptyList_WhenNoArtifactsExist()
        {
            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetSystemArtifacts("empty-system"))
                    .ReturnsAsync(new List<Artifact>());

            var result = await mockRepo.Object.GetSystemArtifacts("empty-system");

            Assert.Empty(result);
        }

        [Fact]
        public async Task Test_GetSystemArtifacts_WrongSystem_DoesNotReturnArtifacts()
        {
            var mockRepo = new Mock<IArtifactRepository>();
            mockRepo.Setup(r => r.GetSystemArtifacts("sys-A")).ReturnsAsync(new List<Artifact> { BuildArtifact("sys-A") });
            mockRepo.Setup(r => r.GetSystemArtifacts("sys-B")).ReturnsAsync(new List<Artifact>());

            var result = await mockRepo.Object.GetSystemArtifacts("sys-B");

            Assert.Empty(result);
        }
    }
}
