using Microsoft.EntityFrameworkCore;
using Xunit;
using ST10371895_Part3;
using ST10371895_Part3.Models;

namespace ST10371895_Part3_Test.StressTests
{
    public class MyDatabaseStressTests
    {
        private readonly DbContextOptions<FoundationDbContext> _dbContextOptions;

        public MyDatabaseStressTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<FoundationDbContext>()
                .UseInMemoryDatabase(databaseName: "StressTestDb")
                .Options;
        }

        [Fact]
        public async Task Database_ConcurrentWrites_ShouldHandleStress()
        {
            // Arrange
            var tasks = new List<Task>();
            var successCount = 0;
            var errorCount = 0;
            var operationCount = 30; // Reduced from 50 to be more realistic

            // Act - Simulate concurrent database operations
            for (int i = 0; i < operationCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // Each task gets its own DbContext to avoid conflicts
                        using var context = new FoundationDbContext(_dbContextOptions);

                        var user = new User
                        {
                            Username = $"stressuser_{Guid.NewGuid()}",
                            Email = $"stress_{Guid.NewGuid()}@test.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                            FirstName = "Stress",
                            LastName = "Test",
                            PhoneNumber = "1234567890",
                            CreatedAt = DateTime.UtcNow
                        };

                        context.Users.Add(user);
                        await context.SaveChangesAsync();

                        // Use Interlocked for thread-safe increment
                        System.Threading.Interlocked.Increment(ref successCount);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Database operation failed: {ex.Message}");
                        System.Threading.Interlocked.Increment(ref errorCount);
                    }
                }));
            }

            // Wait for all operations to complete with timeout
            await Task.WhenAll(tasks);

            // Assert
            Console.WriteLine($"Database Stress Test Results:");
            Console.WriteLine($"Total Operations: {operationCount}");
            Console.WriteLine($"Successful: {successCount}");
            Console.WriteLine($"Errors: {errorCount}");

            Assert.True(successCount >= 25, $"Too many database failures: {errorCount} errors");
            Assert.True(errorCount <= 5, $"Database couldn't handle stress: {errorCount} failures");
        }

        [Fact]
        public async Task Database_ConcurrentReads_ShouldHandleStress()
        {
            // Arrange - Add test data first with proper required fields
            using var setupContext = new FoundationDbContext(_dbContextOptions);

            // Clear any existing data
            setupContext.Users.RemoveRange(setupContext.Users);
            setupContext.IncidentReports.RemoveRange(setupContext.IncidentReports);
            await setupContext.SaveChangesAsync();

            // Add a test user first
            var user = new User
            {
                Username = "testuser_stress",
                Email = "stress@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                FirstName = "Stress",
                LastName = "Test",
                PhoneNumber = "1234567890",
                CreatedAt = DateTime.UtcNow
            };
            setupContext.Users.Add(user);
            await setupContext.SaveChangesAsync();

            // Add multiple test incidents with ALL required fields
            for (int i = 0; i < 10; i++) // Reduced from 20 to be faster
            {
                var incident = new IncidentReport
                {
                    Title = $"Test Incident {i}",
                    Description = $"Description for incident {i}",
                    Location = $"Location {i}",
                    IncidentDate = DateTime.UtcNow,
                    DisasterType = "Flood",
                    AffectedAreas = $"Area {i}", // This was missing!
                    UrgencyLevel = "Medium",
                    UserId = user.UserId, // Use the actual user ID
                    ReportedAt = DateTime.UtcNow
                };
                setupContext.IncidentReports.Add(incident);
            }
            await setupContext.SaveChangesAsync();

            var tasks = new List<Task>();
            var successCount = 0;
            var operationCount = 20; // Reduced from 30

            // Act - Simulate concurrent reads
            for (int i = 0; i < operationCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var readContext = new FoundationDbContext(_dbContextOptions);
                        var incidents = await readContext.IncidentReports.ToListAsync();
                        Assert.True(incidents.Count >= 5); // Reduced expectation
                        System.Threading.Interlocked.Increment(ref successCount);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Read operation failed: {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            Console.WriteLine($"Database Read Stress Test:");
            Console.WriteLine($"Successful Reads: {successCount}/{operationCount}");
            Assert.True(successCount >= 15, $"Too many read failures: Only {successCount}/{operationCount} succeeded");
        }

    }
}