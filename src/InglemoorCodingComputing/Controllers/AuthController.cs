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
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserAuthService userAuthService, IUserService userService, IEmailService emailService, ILogger<AuthController> logger)
    {
        _userAuthService = userAuthService;
        _userService = userService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody]LoginRequest request)
    {
        await HttpContext.SignOutAsync();
        var result = await _userAuthService.AuthenticateAsync(request.Email, request.Password);
        if (result is null)
            return BadRequest("Invalid Credentials.");

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
        };
        if (result.IsAdmin)
            claims.Add(new(ClaimTypes.Role, "Admin"));
        if (result.Verified)
            claims.Add(new(ClaimTypes.Role, "VerifiedEmail"));
        ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        
        _logger.LogInformation($"User with email: '{result.Email}' ({result.Id}) logged in.");
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(claimsIdentity), new() { IsPersistent = request.RememberMe });
        return Ok();
    }

    [HttpPost("logout")]
    public Task LogoutAsync() =>
        HttpContext.SignOutAsync();

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody]RegisterRequest request)
    {
        await HttpContext.SignOutAsync();
        var userAuth = await _userAuthService.AddUserAsync(request.Email, request.Password);
        if (userAuth is null)
        {
            _logger.LogInformation($"User registration with email: '{request.Email}' failed. Email in use.");
            return BadRequest("Email in use.");
        }

        // Send an email
        try
        {
            _emailService.Send(request.Email, "Confirm email for Inglemoor Coding & Computing Club", $"Hi {request.FirstName},\nVerify your email/studentid here: https://{Request.Host}/authentication/verify-email?token={userAuth.VerificationToken}\nThank You\n\nIgnore this email if not expected.");
        }
        catch
        {
            _logger.LogError($"User registration confirmation email for '{request.Email}' ({userAuth.Id}) failed to send");
        }

        // Create user
        await _userService.CreateUser(new()
        {
            Id = userAuth.Id,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            GraduationYear = AppUser.GradeLevelToGraduationYear(request.GradeLevel),
            CreatedDate = DateTime.UtcNow,
        });

        _logger.LogError($"User '{request.FirstName} {request.LastName}' ({userAuth.Id}) registered.");

        // Log in
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, userAuth.Id.ToString()),
        };

        ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(claimsIdentity));
        return Ok();
    }
}
