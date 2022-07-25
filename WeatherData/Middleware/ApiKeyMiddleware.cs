using Microsoft.Extensions.Caching.Memory;

namespace WeatherData.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly string _apiKeyHeaderName;
        private readonly List<string> _apiKeys;
        private readonly IRateLimitManager _rateLimitManager;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config, IRateLimitManager rateLimitManager)
        {
            _next = next;
            _rateLimitManager = rateLimitManager;
            _apiKeyHeaderName = config["ApiKeyHeader"];
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

            if (_rateLimitManager.IsRateLimitHit(clientKey))
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Rate limit exceeded");
                return;
            }

            await _next(context);
        }
    }
}
