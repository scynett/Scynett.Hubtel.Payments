using Microsoft.AspNetCore.Mvc;

namespace Scynett.Hubtel.Payments.Samples.Web.Controllers;

[ApiController]
[Route("[controller]")]
internal sealed class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
#pragma warning disable CA1822 // Mark members as static
    public IEnumerable<WeatherForecast> Get()
#pragma warning restore CA1822 // Mark members as static
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = 23,
            Summary = Summaries[0]
        })
        .ToArray();
    }
}
