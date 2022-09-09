namespace InglemoorCodingComputing.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserAuthService userAuthService, ILogger<AuthController> logger)
    {
        _userAuthService = userAuthService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody]LoginRequest request)
    {
        await HttpContext.SignOutAsync();
        var result = await _userAuthService.AuthenticateAsync(request.Email.Trim(), request.Password);
        if (result is null)
            return BadRequest("Invalid Credentials.");

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
        };
        if (result.IsAdmin)
            claims.Add(new(ClaimTypes.Role, "Admin"));
        ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        
        _logger.LogInformation($"User with email: '{result.Email}' ({result.Id}) logged in.");
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(claimsIdentity), new() { IsPersistent = request.RememberMe });
        return Ok();
    }

    [HttpPost("logout")]
    public Task LogoutAsync() =>
        HttpContext.SignOutAsync();

}
