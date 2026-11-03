using Xunit;
using openrmf_report_api.Controllers;
using openrmf_report_api.Data;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace tests.Controllers
{
    public class HealthControllerTests
    {
        private readonly Mock<ILogger<HealthController>> _mockLogger;
        private readonly Mock<IReportRepository> _mockReportRepo;
        private readonly HealthController _healthController;

        public HealthControllerTests()
        {
            _mockLogger = new Mock<ILogger<HealthController>>();
            _mockReportRepo = new Mock<IReportRepository>();
            _healthController = new HealthController(_mockReportRepo.Object, _mockLogger.Object);
        }

        // ---- Pass Tests ----

        [Fact]
        public void Test_HealthControllerIsNotNull()
        {
            Assert.NotNull(_healthController);
        }

        [Fact]
        public void Test_HealthControllerGetReturnsOkWhenHealthy()
        {
            _mockReportRepo.Setup(r => r.HealthStatus()).Returns(true);
            var result = _healthController.Get();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("ok", okResult.Value);
        }

        [Fact]
        public void Test_HealthControllerGetResultIsNotNullWhenHealthy()
        {
            _mockReportRepo.Setup(r => r.HealthStatus()).Returns(true);
            var result = _healthController.Get();
            Assert.NotNull(result);
        }

        // ---- Fail / Negative Tests ----

        [Fact]
        public void Test_HealthControllerGetReturnsBadRequestWhenUnhealthy()
        {
            _mockReportRepo.Setup(r => r.HealthStatus()).Returns(false);
            var result = _healthController.Get();
            var badResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badResult.StatusCode);
        }

        [Fact]
        public void Test_HealthControllerGetReturnsBadRequestOnException()
        {
            _mockReportRepo.Setup(r => r.HealthStatus()).Throws(new System.Exception("DB connection failure"));
            var result = _healthController.Get();
            var badResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badResult.StatusCode);
        }
    }
}
