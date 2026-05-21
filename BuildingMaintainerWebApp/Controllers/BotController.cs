using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BuildingMaintainerWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class BotController : ControllerBase
{
    private readonly ILogger<BotController> _logger;

    public BotController(ILogger<BotController> logger)
    {
        _logger = logger;
    }

    [HttpPost("/handle-message")]
    public IActionResult HandleMessage([FromBody] JsonElement payload)
    {
        _logger.LogInformation("Received message event from WAHA:\n{Payload}", payload.ToString());
        return Ok();
    }
}
