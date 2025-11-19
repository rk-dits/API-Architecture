
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace CoreWorkflowService.Api.Controllers;

/// <summary>
/// Provides weather forecast data for demonstration purposes.
/// </summary>
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets a list of weather forecasts for the next 5 days.
    /// </summary>
    /// <remarks>
    /// Returns a random set of weather forecast data for demonstration and testing purposes.
    /// </remarks>
    /// <returns>List of weather forecast objects.</returns>
    /// <response code="200">Returns the list of weather forecasts.</response>
    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType(typeof(IEnumerable<WeatherForecast>), 200)]
    [SwaggerOperation(
        Summary = "Gets weather forecasts for the next 5 days.",
        Description = "Returns a random set of weather forecast data for demonstration and testing purposes.",
        OperationId = "GetWeatherForecast",
        Tags = new[] { "WeatherForecast" }
    )]
    [SwaggerResponseExample(200, typeof(WeatherForecastExampleProvider))]
    public IEnumerable<WeatherForecast> Get()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = GetSecureRandomInt(rng, -20, 55),
            Summary = Summaries[GetSecureRandomInt(rng, 0, Summaries.Length)]
        })
        .ToArray();
    }

    private static int GetSecureRandomInt(System.Security.Cryptography.RandomNumberGenerator rng, int minValue, int maxValue)
    {
        byte[] randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        int randomInt = Math.Abs(BitConverter.ToInt32(randomBytes, 0));
        return minValue + (randomInt % (maxValue - minValue));
    }
}

/// <summary>
/// Example provider for WeatherForecast response.
/// </summary>
public class WeatherForecastExampleProvider : IExamplesProvider<IEnumerable<WeatherForecast>>
{
    public IEnumerable<WeatherForecast> GetExamples()
    {
        return new List<WeatherForecast>
        {
            new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = 23,
                Summary = "Mild"
            },
            new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                TemperatureC = 17,
                Summary = "Cool"
            }
        };
    }
}
