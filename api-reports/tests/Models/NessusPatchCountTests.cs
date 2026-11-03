using Xunit;
using openrmf_report_api.Models;

namespace tests.Models
{
    public class NessusPatchCountTests
    {
        // ---- Pass Tests ----

        [Fact]
        public void Test_NewNessusPatchCountIsNotNull()
        {
            NessusPatchCount count = new NessusPatchCount();
            Assert.NotNull(count);
        }

        [Fact]
        public void Test_NessusPatchCountDefaultValuesAreZero()
        {
            NessusPatchCount count = new NessusPatchCount();
            Assert.Equal(0, count.totalCriticalOpen);
            Assert.Equal(0, count.totalHighOpen);
            Assert.Equal(0, count.totalMediumOpen);
            Assert.Equal(0, count.totalLowOpen);
            Assert.Equal(0, count.totalInfoOpen);
        }

        [Fact]
        public void Test_NessusPatchCountCriticalCanBeSet()
        {
            NessusPatchCount count = new NessusPatchCount { totalCriticalOpen = 5 };
            Assert.Equal(5, count.totalCriticalOpen);
        }

        [Fact]
        public void Test_NessusPatchCountHighCanBeSet()
        {
            NessusPatchCount count = new NessusPatchCount { totalHighOpen = 10 };
            Assert.Equal(10, count.totalHighOpen);
        }

        [Fact]
        public void Test_NessusPatchCountMediumCanBeSet()
        {
            NessusPatchCount count = new NessusPatchCount { totalMediumOpen = 20 };
            Assert.Equal(20, count.totalMediumOpen);
        }

        [Fact]
        public void Test_NessusPatchCountLowCanBeSet()
        {
            NessusPatchCount count = new NessusPatchCount { totalLowOpen = 3 };
            Assert.Equal(3, count.totalLowOpen);
        }

        [Fact]
        public void Test_NessusPatchCountInfoCanBeSet()
        {
            NessusPatchCount count = new NessusPatchCount { totalInfoOpen = 100 };
            Assert.Equal(100, count.totalInfoOpen);
        }

        [Fact]
        public void Test_NessusPatchCountAllFieldsSetCorrectly()
        {
            NessusPatchCount count = new NessusPatchCount
            {
                totalCriticalOpen = 1,
                totalHighOpen = 2,
                totalMediumOpen = 3,
                totalLowOpen = 4,
                totalInfoOpen = 5
            };
            Assert.Equal(1, count.totalCriticalOpen);
            Assert.Equal(2, count.totalHighOpen);
            Assert.Equal(3, count.totalMediumOpen);
            Assert.Equal(4, count.totalLowOpen);
            Assert.Equal(5, count.totalInfoOpen);
        }

        // ---- Fail / Negative Tests ----

        [Fact]
        public void Test_CriticalCountIsNotHighCount()
        {
            NessusPatchCount count = new NessusPatchCount { totalCriticalOpen = 5, totalHighOpen = 10 };
            Assert.NotEqual(count.totalCriticalOpen, count.totalHighOpen);
        }

        [Fact]
        public void Test_DefaultCountIsNotPositive()
        {
            NessusPatchCount count = new NessusPatchCount();
            Assert.Equal(0, count.totalCriticalOpen);
            Assert.False(count.totalCriticalOpen > 0);
        }
    }
}
