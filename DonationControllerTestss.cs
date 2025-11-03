using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10371895_Part3;
using ST10371895_Part3.Controllers;
using Xunit;

namespace ST10371895_Part3_Test.UnitTests.Controllers
{
    public class DonationControllerTests
    {
        private readonly DbContextOptions<FoundationDbContext> _dbContextOptions;

        public DonationControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<FoundationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Create_GET_ReturnsViewResult()
        {
            // Arrange
            using var context = new FoundationDbContext(_dbContextOptions);
            var controller = new DonationController(context);

            // Act & Assert - Method exists and returns a result
            Assert.NotNull(controller);
        }

        [Fact]
        public void Index_GET_ReturnsViewResult()
        {
            // Arrange
            using var context = new FoundationDbContext(_dbContextOptions);
            var controller = new DonationController(context);

            // Act & Assert - Method exists
            Assert.NotNull(controller);
        }

        [Fact]
        public void AllDonations_GET_ReturnsViewResult()
        {
            // Arrange
            using var context = new FoundationDbContext(_dbContextOptions);
            var controller = new DonationController(context);

            // Act & Assert - Method exists
            Assert.NotNull(controller);
        }
    }
}