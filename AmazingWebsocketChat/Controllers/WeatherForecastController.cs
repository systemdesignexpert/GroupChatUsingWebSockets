using AmazingWebsocketChat.Database;
using Microsoft.AspNetCore.Mvc;

namespace AmazingWebsocketChat.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private IDatabaseClient dbClient;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
        dbClient = SQLDatabaseClient.getInstance();
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpPost]
    [Route("addforecast")]
    public IEnumerable<WeatherForecast> Post([FromBody] WeatherForecast forecast)
    {
        IEnumerable<WeatherForecast> forecasts =  Enumerable.Range(1, 5).Select(
            index => new WeatherForecast
                            {
                                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                TemperatureC = Random.Shared.Next(-20, 55),
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                            })
            .ToArray();

        forecasts = forecasts.Append(forecast);
        return forecasts;

    }

    [HttpGet]
    [Route("getchats")]
    public async Task<List<ChatResponse>> GetChats()
    {
        List<ChatResponse> response = await dbClient.getChats();
        return response;
    }
}

