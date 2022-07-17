using Microsoft.AspNetCore.Mvc;

namespace WeatherData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Description([FromQuery] string city, [FromQuery] string country)
        {
            try
            {
                var description = await _weatherService.GetWeatherDescription(city, country);

                if (description.Count == 0)
                {
                    return NoContent();
                }

                return Ok(description);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
