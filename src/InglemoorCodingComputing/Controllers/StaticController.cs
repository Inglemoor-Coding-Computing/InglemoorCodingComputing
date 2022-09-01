namespace InglemoorCodingComputing.Controllers;

using Microsoft.AspNetCore.Mvc;

[Route("static")]
[ApiController]
public class StaticController : ControllerBase
{
    private readonly IStaticResourceService _staticResourceService;

    public StaticController(IStaticResourceService staticResourceService)
    {
        _staticResourceService = staticResourceService;
    }

    [HttpGet("{filename}")]
    public async Task<IActionResult> GetAsync(string filename) =>
        await _staticResourceService.DownloadAsync(filename) switch
        {
            Stream stream => File(stream, MimeKit.MimeTypes.GetMimeType(filename)),
            _ => NotFound()
        };
}