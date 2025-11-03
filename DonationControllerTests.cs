using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;
using Moq;

namespace GiftOfTheGiversFoundationTest.Unit_test
{
    public class DonationControllerTests : IDisposable
    {
        private readonly DbContextOptions<FoundationDbContext> _dbContextOptions;
        private FoundationDbContext _context;

        public DonationControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<FoundationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new FoundationDbContext(_dbContextOptions);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        private DonationController CreateAuthenticatedController(int userId = 1, string username = "testuser")
        {
            var controller = new DonationController(_context);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Ensure TempData is available to avoid NullReference when controller uses TempData
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewWithUserDonations()
        {
            // Arrange
            var user = new User { UserId = 1, Username = "testuser", Email = "test@example.com", FirstName = "Test", LastName = "User", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), PhoneNumber = "0000000000" };
            _context.Users.Add(user);

            var donations = new List<Donation>
            {
                new Donation { DonationId = 1, UserId = 1, DonationType = "Food", ItemDescription = "Rice", Quantity = 10, Unit = "kg", DonationDate = DateTime.UtcNow.AddDays(-1), TargetArea = "Local", SpecialInstructions = "None" },
                new Donation { DonationId = 2, UserId = 1, DonationType = "Clothing", ItemDescription = "Winter jackets", Quantity = 5, Unit = "pieces", DonationDate = DateTime.UtcNow, TargetArea = "Local", SpecialInstructions = "Handle with care" }
            };
            _context.Donations.AddRange(donations);
            await _context.SaveChangesAsync();

            var controller = CreateAuthenticatedController(1);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Donation>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.All(model, d => Assert.Equal(1, d.UserId));
        }

        [Fact]
        public async Task Index_NoDonations_ReturnsEmptyList()
        {
            // Arrange
            var user = new User { UserId = 1, Username = "testuser", Email = "test@example.com", FirstName = "Test", LastName = "User", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), PhoneNumber = "0000000000" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var controller = CreateAuthenticatedController(1);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Donation>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public void Create_GET_ReturnsView()
        {
            // Arrange
            var controller = CreateAuthenticatedController();

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task Create_POST_ValidModel_CreatesDonationAndRedirects()
        {
            // Arrange
            var user = new User { UserId = 1, Username = "testuser", Email = "test@example.com", FirstName = "Test", LastName = "User", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), PhoneNumber = "0000000000" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var controller = CreateAuthenticatedController(1);
            var model = new DonationViewModel
            {
                DonationType = "Medical",
                ItemDescription = "First aid kits",
                Quantity = 15,
                Unit = "kits",
                TargetArea = "Flood affected region",
                SpecialInstructions = "Urgent - needed by Friday"
            };

            // Act
            var result = await controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var donation = await _context.Donations.FirstOrDefaultAsync();
            Assert.NotNull(donation);
            Assert.Equal("Medical", donation.DonationType);
            Assert.Equal("First aid kits", donation.ItemDescription);
            Assert.Equal(15, donation.Quantity);
            Assert.Equal("kits", donation.Unit);
            Assert.Equal("Flood affected region", donation.TargetArea);
            Assert.Equal("Urgent - needed by Friday", donation.SpecialInstructions);
            Assert.Equal(1, donation.UserId);
            Assert.Equal("Pending", donation.Status);
        }

        [Fact]
        public async Task Create_POST_InvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            var controller = CreateAuthenticatedController();
            var model = new DonationViewModel
            {
                DonationType = "", // Invalid - required
                ItemDescription = "Test",
                Quantity = 0, // Invalid - must be greater than 0
                Unit = "kg",
                TargetArea = "Test Area"
            };
            controller.ModelState.AddModelError("DonationType", "Required");
            controller.ModelState.AddModelError("Quantity", "Must be greater than 0");

            // Act
            var result = await controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task AllDonations_ReturnsAllDonationsWithUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, Username = "user1", Email = "user1@example.com", FirstName = "John", LastName = "Doe", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), PhoneNumber = "0000000000" },
                new User { UserId = 2, Username = "user2", Email = "user2@example.com", FirstName = "Jane", LastName = "Smith", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), PhoneNumber = "0000000000" }
            };
            _context.Users.AddRange(users);

            var donations = new List<Donation>
            {
                new Donation { DonationId = 1, UserId = 1, DonationType = "Food", ItemDescription = "Rice", Quantity = 10, Unit = "kg", DonationDate = DateTime.UtcNow.AddDays(-2), TargetArea = "Region A", SpecialInstructions = "None" },
                new Donation { DonationId = 2, UserId = 2, DonationType = "Clothing", ItemDescription = "Jackets", Quantity = 5, Unit = "pieces", DonationDate = DateTime.UtcNow.AddDays(-1), TargetArea = "Region B", SpecialInstructions = "Wash before use" },
                new Donation { DonationId = 3, UserId = 1, DonationType = "Medical", ItemDescription = "Masks", Quantity = 100, Unit = "pieces", DonationDate = DateTime.UtcNow, TargetArea = "Region A", SpecialInstructions = "Pack securely" }
            };
            _context.Donations.AddRange(donations);
            await _context.SaveChangesAsync();

            var controller = CreateAuthenticatedController();

            // Act
            var result = await controller.AllDonations();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Donation>>(viewResult.Model);
            Assert.Equal(3, model.Count());

            // Verify donations are ordered by date descending
            var donationsList = model.ToList();
            Assert.Equal(3, donationsList[0].DonationId); // Most recent
            Assert.Equal(2, donationsList[1].DonationId);
            Assert.Equal(1, donationsList[2].DonationId); // Oldest

            // Verify user information is included
            Assert.NotNull(donationsList[0].User);
            Assert.NotNull(donationsList[1].User);
        }
    }
}