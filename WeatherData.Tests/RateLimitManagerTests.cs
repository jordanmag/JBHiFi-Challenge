using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;

namespace WeatherData.Tests;

public class RateLimitManagerTests
{
    private IMemoryCache _cache;
    private IConfiguration _config;
    private RateLimitManager _rateLimitManager;

    public RateLimitManagerTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();
    }

    [SetUp]
    public void Setup()
    {
        _rateLimitManager = new RateLimitManager(_cache, _config);
    }

    [TestCase(3)]
    [TestCase(1)]
    public void RateLimitUpdate_Increments_CountByOne(int count)
    {
        var key = "c4236b3b569840f6869bad373fbd7e7e";
        var requests = new ClientApiRequests
        {
            LastSuccessfulResponseTime = DateTime.Now,
            SuccessfulRequests = count
        };
        _cache.Set(key, requests);
    
        _rateLimitManager.UpdateRateLimit(key);
        var result = _cache.Get<ClientApiRequests>(key);
    
        Assert.That(result.SuccessfulRequests, Is.EqualTo(count + 1));
    }
    
    [Test]
    public void RateLimitUpdate_WhenLimitHit_ResetsCount()
    {
        var key = "c4236b3b569840f6869bad373fbd7e7e";
        var requests = new ClientApiRequests
        {
            LastSuccessfulResponseTime = DateTime.Now,
            SuccessfulRequests = 5
        };
        _cache.Set(key, requests);
    
        _rateLimitManager.UpdateRateLimit(key);
        var result = _cache.Get<ClientApiRequests>(key);
    
        Assert.That(result.SuccessfulRequests, Is.EqualTo(1));
    }

    [TestCase(10)]
    [TestCase(50)]
    public void IsRateLimitHit_MaxRequestsForDuration_Returns_True(int minutes)
    {
        var key = "c4236b3b569840f6869bad373fbd7e7e";
        var requests = new ClientApiRequests
        {
            LastSuccessfulResponseTime = DateTime.Now.AddMinutes(-minutes),
            SuccessfulRequests = 5
        };
        _cache.Set(key, requests);
        
        var result = _rateLimitManager.IsRateLimitHit(key);
    
        Assert.That(result, Is.EqualTo(true));
    }
}