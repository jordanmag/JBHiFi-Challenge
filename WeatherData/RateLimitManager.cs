using Microsoft.Extensions.Caching.Memory;

namespace WeatherData;

public class RateLimitManager : IRateLimitManager
{
    private readonly IMemoryCache _cache;
    private readonly int _rateLimit;
    private readonly double _rateLimitMinsDuration;
    
    public RateLimitManager(IMemoryCache cache, IConfiguration config)
    {
        _cache = cache;
        _rateLimit = int.Parse(config["RateLimit"]);
        _rateLimitMinsDuration = double.Parse(config["RateLimitDuration"]);
    }
    
    public void UpdateRateLimit(string key)
    {
        var requests = _cache.Get<ClientApiRequests>(key);

        if (requests != null)
        {
            requests.LastSuccessfulResponseTime = DateTime.Now;
            if (requests.SuccessfulRequests >= _rateLimit)
            {
                requests.SuccessfulRequests = 1;
            }
            else
            {
                requests.SuccessfulRequests++;
            }
            _cache.Set(key, requests);
        }
        else
        {
            var newRequest = new ClientApiRequests
            {
                LastSuccessfulResponseTime = DateTime.Now,
                SuccessfulRequests = 1
            };
            _cache.Set(key, newRequest);
        }
    }

    public bool IsRateLimitHit(string key)
    {
        var clientRequests = _cache.Get<ClientApiRequests>(key);

        return clientRequests != null 
               && DateTime.Now < clientRequests.LastSuccessfulResponseTime.AddMinutes(_rateLimitMinsDuration)
               && clientRequests.SuccessfulRequests >= _rateLimit;
    }
}

public class ClientApiRequests
{
    public DateTime LastSuccessfulResponseTime { get; set; }
    public int SuccessfulRequests { get; set; }
}