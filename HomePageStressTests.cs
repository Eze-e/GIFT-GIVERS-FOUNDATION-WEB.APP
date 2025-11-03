using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using ST10371895_Part3;

namespace ST10371895_Part3_Test.StressTests
{
    public class HomePageStressTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HomePageStressTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HomePage_StressTest_100ConcurrentRequests()
        {
            // Arrange
            var client = _factory.CreateClient();
            var tasks = new List<Task<HttpResponseMessage>>();
            var requestCount = 100;
            var successCount = 0;
            var failedCount = 0;

            // Act - Simulate 100 concurrent requests
            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(client.GetAsync("/"));
            }

            // Wait for all requests to complete
            var responses = await Task.WhenAll(tasks);

            // Assert - Analyze results
            foreach (var response in responses)
            {
                if (response.IsSuccessStatusCode)
                    successCount++;
                else
                    failedCount++;
            }

            // Log results
            Console.WriteLine($"Stress Test Results:");
            Console.WriteLine($"Total Requests: {requestCount}");
            Console.WriteLine($"Successful: {successCount}");
            Console.WriteLine($"Failed: {failedCount}");
            Console.WriteLine($"Success Rate: {(double)successCount / requestCount * 100}%");

            // Requirements: At least 90% success rate under stress
            Assert.True(successCount >= 90, $"Stress test failed: Only {successCount}/{requestCount} requests succeeded");
            Assert.True(failedCount <= 10, $"Too many failures: {failedCount} requests failed");
        }

        [Fact]
        public async Task HomePage_LoadTest_50RequestsIn5Seconds()
        {
            // Arrange
            var client = _factory.CreateClient();
            var tasks = new List<Task<HttpResponseMessage>>();
            var requestCount = 50;
            var startTime = DateTime.Now;

            // Act - Send 50 requests rapidly
            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(client.GetAsync("/"));
            }

            var responses = await Task.WhenAll(tasks);
            var endTime = DateTime.Now;
            var totalTime = (endTime - startTime).TotalSeconds;

            // Assert - Should complete within 5 seconds
            var successCount = responses.Count(r => r.IsSuccessStatusCode);

            Console.WriteLine($"Load Test Results:");
            Console.WriteLine($"Total Time: {totalTime} seconds");
            Console.WriteLine($"Requests per Second: {requestCount / totalTime}");
            Console.WriteLine($"Success Rate: {successCount}/{requestCount}");

            Assert.True(totalTime < 5, $"Load test too slow: {totalTime} seconds for {requestCount} requests");
            Assert.True(successCount >= 45, $"Too many failures: {requestCount - successCount} requests failed");
        }
    }
}