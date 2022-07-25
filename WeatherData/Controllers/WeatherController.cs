using Microsoft.AspNetCore.Mvc;

namespace WeatherData.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IRateLimitManager _rateLimitManager;

        public WeatherController(IWeatherService weatherService, IRateLimitManager rateLimitManager)
        {
            _weatherService = weatherService;
            _rateLimitManager = rateLimitManager;
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
                _rateLimitManager.UpdateRateLimit(HttpContext.Request.Headers["ApiKey"]);
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
