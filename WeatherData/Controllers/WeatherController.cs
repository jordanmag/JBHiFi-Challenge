using Microsoft.AspNetCore.Mvc;

namespace WeatherData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly ILogger<WeatherController> _logger;
        private readonly IWeatherService _weatherService;

        public WeatherController(ILogger<WeatherController> logger, IWeatherService weatherService)
        {
            _logger = logger;
            _weatherService = weatherService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Description([FromQuery] string city, [FromQuery] string country)
        {
            try
            {
                var description = await _weatherService.GetWeatherDescription(city, country);
                return Ok(description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }

        }
    }
}
