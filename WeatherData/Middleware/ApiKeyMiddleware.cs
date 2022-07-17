using Microsoft.Extensions.Caching.Memory;

namespace WeatherData.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        private readonly string _apiKeyHeaderName;
        private readonly int _rateLimit;
        private readonly double _rateLimitMinsDuration;
        private readonly List<string> _apiKeys;

        public ApiKeyMiddleware(RequestDelegate next, IMemoryCache cache, IConfiguration config)
        {
            _next = next;
            _cache = cache;
            _apiKeyHeaderName = config["ApiKeyHeader"];
            _rateLimit = int.Parse(config["RateLimit"]);
            _rateLimitMinsDuration = double.Parse(config["RateLimitDuration"]);
            _apiKeys = config.GetSection("ApiKeys").Get<List<string>>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(_apiKeyHeaderName, out var clientKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api Key was not provided");
                return;
            }

            if (!_apiKeys.Contains(clientKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client");
                return;
            }

            var clientRequests = _cache.Get<ClientApiRequests>(clientKey.ToString());

            if (clientRequests != null 
                && DateTime.Now < clientRequests.LastSuccessfulResponseTime.AddMinutes(_rateLimitMinsDuration)
                && clientRequests.SuccessfulRequests >= _rateLimit)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Rate limit exceeded");
                return;
            }

            UpdateApiRequests(clientKey, clientRequests);
            await _next(context);
        }

        private void UpdateApiRequests(string key, ClientApiRequests requests)
        {
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
                _cache.Set<ClientApiRequests>(key, requests);
            }
            else
            {
                var newRequest = new ClientApiRequests
                {
                    LastSuccessfulResponseTime = DateTime.Now,
                    SuccessfulRequests = 1
                };
                _cache.Set<ClientApiRequests>(key, newRequest);
            }
        }
    }

    public class ClientApiRequests
    {
        public DateTime LastSuccessfulResponseTime { get; set; }
        public int SuccessfulRequests { get; set; }
    }
}
