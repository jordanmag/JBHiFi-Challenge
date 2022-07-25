using Newtonsoft.Json.Linq;
using System.Net;
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

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var token = JObject.Parse(result)["weather"];
                
                var weatherWithDescription = token.ToObject<List<Weather>>()
                    .Where(x => x.Description != null);

                return weatherWithDescription.Select(x => x.Description).ToList();
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException($"City ({city},{country}) not found");
            }
            throw new HttpRequestException("Error calling OpenWeather API");
        }
    }
}
