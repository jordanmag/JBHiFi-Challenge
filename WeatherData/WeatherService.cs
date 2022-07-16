using Newtonsoft.Json.Linq;
using WeatherData.Models;

namespace WeatherData
{
    public class WeatherService : IWeatherService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _openWeatherMapApiKey;

        public WeatherService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _openWeatherMapApiKey = config["OpenWeatherMapApiKey"];
        }

        public async Task<List<string>> GetWeatherDescription(string city, string country)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"http://api.openweathermap.org/data/2.5/weather?q={city},{country}&appid={_openWeatherMapApiKey}");
            response.EnsureSuccessStatusCode();

            var stringResult = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(stringResult)["weather"];
            
            var weatherList = token?.ToObject<List<Weather>>();
            var description = weatherList?.Select(x => x.Description).ToList();
            
            return description;
        }
    }
}
