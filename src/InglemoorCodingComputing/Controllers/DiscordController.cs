namespace InglemoorCodingComputing.Controllers;

[Route("/[controller]")]
[ApiController]
public class DiscordController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Redirect("s/discord");
}
