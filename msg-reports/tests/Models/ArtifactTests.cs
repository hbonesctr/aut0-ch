using Xunit;
using openrmf_msg_report.Models;
using System;
using System.Collections.Generic;

namespace tests.Models
{
    public class ArtifactTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewArtifact_IsNotNull()
        {
            var art = new Artifact();
            Assert.NotNull(art);
        }

        [Fact]
        public void Test_NewArtifact_ChecklistIsInitialized()
        {
            var art = new Artifact();
            Assert.NotNull(art.CHECKLIST);
        }

        [Fact]
        public void Test_NewArtifact_WebDatabaseDefaults()
        {
            var art = new Artifact();
            Assert.False(art.isWebDatabase);
            Assert.Equal(string.Empty, art.webDatabaseSite);
            Assert.Equal(string.Empty, art.webDatabaseInstance);
        }

        [Fact]
        public void Test_ArtifactWithAllProperties_IsValid()
        {
            var art = new Artifact
            {
                created = DateTime.Now,
                systemGroupId = "system123",
                hostName = "WEBSERVER01",
                stigType = "Windows 10 Security Technical Implementation Guide",
                stigRelease = "Release: 23 Benchmark Date: 26 Jan 2023",
                version = "1",
                updatedOn = DateTime.Now,
                createdBy = Guid.NewGuid(),
                updatedBy = Guid.NewGuid(),
                rawChecklist = "<CHECKLIST/>",
                isWebDatabase = true,
                webDatabaseSite = "MySite",
                webDatabaseInstance = "MyInstance"
            };

            Assert.NotNull(art);
            Assert.NotEmpty(art.systemGroupId);
            Assert.NotEmpty(art.hostName);
            Assert.NotEmpty(art.stigType);
            Assert.NotEmpty(art.stigRelease);
            Assert.NotEmpty(art.version);
            Assert.True(art.updatedOn.HasValue);
            Assert.NotEqual(Guid.Empty, art.createdBy);
            Assert.True(art.updatedBy.HasValue);
            Assert.True(art.isWebDatabase);
            Assert.Equal("MySite", art.webDatabaseSite);
            Assert.Equal("MyInstance", art.webDatabaseInstance);
        }

        [Fact]
        public void Test_ArtifactTitle_ComputedFromHostStigVersionRelease()
        {
            var art = new Artifact
            {
                hostName = "MYHOST",
                stigType = "Google Chrome",
                version = "2",
                stigRelease = "R5"
            };

            Assert.Equal("MYHOST-Google Chrome-V2-R5", art.title);
        }

        [Fact]
        public void Test_ArtifactTitle_UsesUnknownWhenHostNameIsEmpty()
        {
            var art = new Artifact
            {
                hostName = "",
                stigType = "STIG",
                version = "1",
                stigRelease = "R1"
            };

            Assert.StartsWith("Unknown-", art.title);
        }

        [Fact]
        public void Test_ArtifactInternalIdString_IsString()
        {
            var art = new Artifact();
            Assert.NotNull(art.InternalIdString);
        }

        [Fact]
        public void Test_ArtifactTags_CanBeAssigned()
        {
            var art = new Artifact
            {
                tags = new List<string> { "tag1", "tag2" }
            };

            Assert.NotNull(art.tags);
            Assert.Equal(2, art.tags.Count);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_ArtifactSystemGroupId_NullWhenNotSet()
        {
            var art = new Artifact();
            Assert.Null(art.systemGroupId);
        }

        [Fact]
        public void Test_ArtifactCreatedBy_IsEmptyGuidWhenNotSet()
        {
            var art = new Artifact();
            Assert.Equal(Guid.Empty, art.createdBy);
        }

        [Fact]
        public void Test_ArtifactUpdatedOn_IsNullWhenNotSet()
        {
            var art = new Artifact();
            Assert.False(art.updatedOn.HasValue);
        }

        [Fact]
        public void Test_ArtifactTags_IsNullWhenNotSet()
        {
            var art = new Artifact();
            Assert.Null(art.tags);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Test_ArtifactTitle_WithWhitespaceStigType_IsHandled(string stigType)
        {
            var art = new Artifact
            {
                hostName = "HOST",
                stigType = stigType,
                version = "1",
                stigRelease = "R1"
            };

            // title should still be constructed; stigType portion is trimmed/empty
            Assert.NotNull(art.title);
        }
    }
}
