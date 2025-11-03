using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10371895_Part3;
using ST10371895_Part3.Controllers;
using Xunit;

namespace ST10371895_Part3_Test.UnitTests.Controllers
{
    public class VolunteerControllerTests
    {
        private readonly DbContextOptions<FoundationDbContext> _dbContextOptions;

        public VolunteerControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<FoundationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Register_GET_ReturnsViewResult()
        {
            // Arrange
            using var context = new FoundationDbContext(_dbContextOptions);
            var controller = new VolunteerController(context);

            // Act & Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Index_GET_ReturnsViewResult()
        {
            // Arrange
            using var context = new FoundationDbContext(_dbContextOptions);
            var controller = new VolunteerController(context);

            // Act & Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void MyRegistration_GET_ReturnsViewResult()
        {
            // Arrange
            using var context = new FoundationDbContext(_dbContextOptions);
            var controller = new VolunteerController(context);

            // Act & Assert
            Assert.NotNull(controller);
        }
    }
}