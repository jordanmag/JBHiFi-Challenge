using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using WeatherData.Models;

namespace WeatherData.Tests
{
    public class WeatherServiceTests
    {
        private IConfiguration _config;
        private IWeatherService _weatherService;
        private HttpResponseMessage _response;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<IHttpClientFactory> _mockHttpFactory;
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        [TestCase("broken clouds")]
        [TestCase("clear sky")]
        public async Task GetWeatherDescription_Returns_Description(string description)
        {
            ArrangeWeatherResponse(description);
            ArrangeWeatherService();

            var result = await _weatherService.GetWeatherDescription("city", "country");
            
            Assert.That(result.FirstOrDefault, Is.EqualTo(description));
        }

        [Test]
        public async Task MissingWeatherDescription_Returns_Empty()
        {
            ArrangeWeatherResponseWithNoDescription(); 
            ArrangeWeatherService();
            
            var result = await _weatherService.GetWeatherDescription("city", "country");
            
            Assert.That(result, Is.EqualTo(new List<string>()));
        }

        [Test]
        public void WeatherApi_CityNotFound_Returns_ArgumentException()
        {
            ArrangeNotFoundResponse();
            ArrangeWeatherService();

            Assert.ThrowsAsync<ArgumentException>(async () => await _weatherService.GetWeatherDescription("city", "country"));
        }

        [Test]
        public void WeatherApi_Error_Returns_HttpRequestException()
        {
            ArrangeServerErrorResponse();
            ArrangeWeatherService();

            Assert.ThrowsAsync<HttpRequestException>(async () => await _weatherService.GetWeatherDescription("city", "country"));
        }

        private void ArrangeWeatherService()
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_response);

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            _mockHttpFactory = new Mock<IHttpClientFactory>();
            _mockHttpFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            _weatherService = new WeatherService(_mockHttpFactory.Object, _config);
        }

        private void ArrangeWeatherResponse(string description)
        {
            _response = new HttpResponseMessage
            {
                Content = JsonContent.Create(new
                {
                    weather = new List<Weather>
                    {
                        new()
                        {
                            Id = 800,
                            Main = "clear",
                            Description = description,
                            Icon = "01n"
                        }
                    }
                })
            };
        }

        private void ArrangeWeatherResponseWithNoDescription()
        {
            _response = new HttpResponseMessage
            {
                Content = JsonContent.Create(new
                {
                    weather = new List<Weather>
                    {
                        new()
                        {
                            Id = 800,
                            Main = "clear",
                            Icon = "01n"
                        }
                    }
                })
            };
        }

        private void ArrangeNotFoundResponse() => _response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
        private void ArrangeServerErrorResponse() => _response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

    }
}
