
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Cryptography;

namespace IntegrationHub.Api.Controllers;

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
        using var rng = RandomNumberGenerator.Create();
        var temperatureBytes = new byte[4];
        var summaryIndexBytes = new byte[4];

        return Enumerable.Range(1, 5).Select(index =>
        {
            rng.GetBytes(temperatureBytes);
            rng.GetBytes(summaryIndexBytes);

            var temperature = Math.Abs(BitConverter.ToInt32(temperatureBytes, 0)) % 75 - 20; // Range: -20 to 54
            var summaryIndex = Math.Abs(BitConverter.ToInt32(summaryIndexBytes, 0)) % Summaries.Length;

            return new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = temperature,
                Summary = Summaries[summaryIndex]
            };
        })
        .ToArray();
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
