using Xunit;
using openrmf_msg_report.Models;
using System;

namespace tests.Models
{
    public class SystemGroupTests
    {
        // ── Pass tests ────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewSystemGroup_IsNotNull()
        {
            var sys = new SystemGroup();
            Assert.NotNull(sys);
        }

        [Fact]
        public void Test_NewSystemGroup_NumberOfChecklistsDefaultsToZero()
        {
            var sys = new SystemGroup();
            Assert.Equal(0, sys.numberOfChecklists);
        }

        [Fact]
        public void Test_NewSystemGroup_NullableDatesAreUnsetByDefault()
        {
            var sys = new SystemGroup();
            Assert.False(sys.updatedOn.HasValue);
            Assert.False(sys.lastComplianceCheck.HasValue);
        }

        [Fact]
        public void Test_SystemGroup_AllProperties_AreSetCorrectly()
        {
            var now = DateTime.Now;
            var sys = new SystemGroup
            {
                created = now,
                title = "My System Title",
                description = "This is my System description.",
                numberOfChecklists = 5,
                nessusFilename = "servers.nessus",
                rawNessusFile = "<NessusClientData_v2/>",
                updatedOn = now,
                lastComplianceCheck = now,
                createdBy = Guid.NewGuid(),
                updatedBy = Guid.NewGuid()
            };

            Assert.Equal("My System Title", sys.title);
            Assert.Equal("This is my System description.", sys.description);
            Assert.Equal(5, sys.numberOfChecklists);
            Assert.Equal("servers.nessus", sys.nessusFilename);
            Assert.NotEmpty(sys.rawNessusFile);
            Assert.True(sys.updatedOn.HasValue);
            Assert.True(sys.lastComplianceCheck.HasValue);
            Assert.NotEqual(Guid.Empty, sys.createdBy);
            Assert.True(sys.updatedBy.HasValue);
        }

        [Fact]
        public void Test_SystemGroup_InternalIdString_IsString()
        {
            var sys = new SystemGroup();
            Assert.NotNull(sys.InternalIdString);
        }

        // ── Fail / negative tests ─────────────────────────────────────────────

        [Fact]
        public void Test_NewSystemGroup_TitleIsNullByDefault()
        {
            var sys = new SystemGroup();
            Assert.Null(sys.title);
        }

        [Fact]
        public void Test_NewSystemGroup_CreatedByIsEmptyGuidByDefault()
        {
            var sys = new SystemGroup();
            Assert.Equal(Guid.Empty, sys.createdBy);
        }

        [Fact]
        public void Test_SystemGroup_NumberOfChecklists_DoesNotEqualWrongValue()
        {
            var sys = new SystemGroup { numberOfChecklists = 3 };
            Assert.NotEqual(5, sys.numberOfChecklists);
        }
    }
}
