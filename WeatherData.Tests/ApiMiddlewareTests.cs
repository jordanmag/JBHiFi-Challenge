using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using WeatherData.Middleware;

namespace WeatherData.Tests
{
    public class ApiMiddlewareTests
    {
        private DefaultHttpContext _httpContext;
        private IConfiguration _config;
        private ApiKeyMiddleware _middleware;
        private Mock<IRateLimitManager> _mockRateLimitManager;

        public ApiMiddlewareTests()
        {
            _mockRateLimitManager = new Mock<IRateLimitManager>();
            _httpContext = new DefaultHttpContext();
        }

        [SetUp]
        public void Setup()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();

            _mockRateLimitManager.Setup(x => x.IsRateLimitHit(It.IsAny<string>()))
                .Returns(true);
            
            _middleware = new ApiKeyMiddleware(next: (innerHttpContext) => Task.CompletedTask,
            _config,
            _mockRateLimitManager.Object);
        }

        [Test]
        public async Task MissingApiKeyHeader_Returns_401()
        {
            await _middleware.InvokeAsync(_httpContext);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(401));
        }

        [TestCase("")]
        [TestCase("e5e3319233cf4f17a0dd33f02984fec1")]
        public async Task InvalidApiKey_Returns_401(string key)
        {
            _httpContext.Request.Headers["ApiKey"] = key;
            await _middleware.InvokeAsync(_httpContext);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(401));
        }

        [TestCase(6)]
        [TestCase(15)]
        public async Task ValidKey_RateLimitHit_Returns_429(int requests)
        {
            _httpContext.Request.Headers["ApiKey"] = "c4236b3b569840f6869bad373fbd7e7e";
            await _middleware.InvokeAsync(_httpContext);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(429));
        }
    }
}
