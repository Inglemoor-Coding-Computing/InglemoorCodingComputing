namespace InglemoorCodingComputing.Controllers;

[Route("s/")]
[ApiController]
public class UrlShortenerController : ControllerBase
{
    private readonly IURLShortenerService _URLShortenerService;

    public UrlShortenerController(IURLShortenerService URLShortenerService)
    {
        _URLShortenerService = URLShortenerService;
    }

    [HttpGet("{shortened}")]
    public async Task<IActionResult> GetAsync(string shortened) =>
        await _URLShortenerService.FindExpandedURLAsync(shortened) switch
        { 
            string x => Redirect(x),
            _ => NotFound($"404: We couldn't find that link. :cry:")
        };

}
