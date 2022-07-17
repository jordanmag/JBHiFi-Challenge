using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using WeatherData.Middleware;

namespace WeatherData.Tests
{
    public class ApiMiddlewareTests
    {
        private DefaultHttpContext _httpContext;
        private IMemoryCache _cache;
        private IConfiguration _config;
        private ApiKeyMiddleware _middleware;

        [SetUp]
        public void Setup()
        {
            _httpContext = new DefaultHttpContext();
            _cache = new MemoryCache(new MemoryCacheOptions());
            
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();
            
            _middleware = new ApiKeyMiddleware(next: (innerHttpContext) =>
            {
                return Task.CompletedTask;
            },
            _cache,
            _config);
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

        [TestCase("c4236b3b569840f6869bad373fbd7e7e")]
        [TestCase("f6af7911a5374dd3a06af2faec8d69ca")]
        public async Task ValidApiKey_Returns_200(string key)
        {
            _httpContext.Request.Headers["ApiKey"] = key;
            await _middleware.InvokeAsync(_httpContext);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
        }

        [TestCase(6)]
        [TestCase(15)]
        public async Task ValidRequest_RateLimitHit_Returns_429(int requests)
        {
            _httpContext.Request.Headers["ApiKey"] = "c4236b3b569840f6869bad373fbd7e7e";
            
            for (int i = 0; i < requests; i++)
            {
                await _middleware.InvokeAsync(_httpContext);
            }
            
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(429));
        }

        [TestCase(60)]
        [TestCase(90)]
        public async Task RateLimitHit_AfterTimeExpires_ResetsCount(int minutes)
        {
            var key = "c4236b3b569840f6869bad373fbd7e7e";
            var requests = new ClientApiRequests
            {
                LastSuccessfulResponseTime = DateTime.Now.AddMinutes(-minutes),
                SuccessfulRequests = 5
            };
            _httpContext.Request.Headers["ApiKey"] = key;
            _cache.Set<ClientApiRequests>(key, requests);

            await _middleware.InvokeAsync(_httpContext);
            var result = _cache.Get<ClientApiRequests>(key);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
            Assert.That(result.SuccessfulRequests, Is.EqualTo(1));
        }
    }
}
