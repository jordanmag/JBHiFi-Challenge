namespace WeatherData;

public interface IRateLimitManager
{
    public void UpdateRateLimit(string key);
    public bool IsRateLimitHit(string key);
}