namespace WeatherData
{
    public interface IWeatherService
    {
        Task<List<string>> GetWeatherDescription(string city, string country);
    }
}
