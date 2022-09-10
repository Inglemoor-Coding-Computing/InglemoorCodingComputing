﻿namespace InglemoorCodingComputing.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserAuthService userAuthService, IUserService userService, ILogger<AuthController> logger)
    {
        _userAuthService = userAuthService;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
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

    [HttpGet("login-google")]
    public async Task LoginGoogleAsync(string? returnUrl)
    {
        await HttpContext.SignOutAsync();
        await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new() { RedirectUri = $"api/auth/sign-in-callback/google{(returnUrl is null ? "" : "?returnUrl=" + Uri.EscapeDataString(returnUrl))}" });
    }

    [HttpGet("sign-in-callback/google")]
    public async Task<IActionResult> GoogleCallbackAsync(string? returnUrl, string? remoteError)
    {
        var firstName = User.FindFirstValue(ClaimTypes.GivenName);
        var lastName = User.FindFirstValue(ClaimTypes.Surname);
        var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var emailAddress = User.FindFirstValue(ClaimTypes.Email);

        if (emailAddress is null || googleId is null)
        {
            await HttpContext.SignOutAsync();
            return Redirect("/authentication/login-failure");
        }

        if (await _userAuthService.AddGoogleUserAsync(emailAddress, googleId) is UserAuth userAuth)
        {
            // Register user
            await _userService.CreateUser(new()
            {
                Id = userAuth.Id,
                Email = emailAddress,
                FirstName = firstName,
                LastName = lastName,
                GraduationYear = -1,
                CreatedDate = DateTime.UtcNow,
            });
        }
        if (await _userAuthService.UserWithGoogleIdAsync(googleId) is UserAuth userAuth2)
        {
            // Login
            // Replace the id claim from google with our own
            var identity = User.Identity as ClaimsIdentity;
            var claim = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

            identity?.RemoveClaim(claim);
            identity?.AddClaim(new(ClaimTypes.NameIdentifier, userAuth2.Id.ToString()));

            if (userAuth2.IsAdmin)
                identity?.AddClaim(new(ClaimTypes.Role, "Admin"));

            if (identity is not null)
                await HttpContext.SignInAsync(new(identity));

            var user = await _userService.ReadUser(userAuth2.Id);

            return Redirect(
                user?.RegistrationIncomplete is true
                ? $"/authentication/register-google{(returnUrl is null ? "" : "?returnUrl=" + Uri.EscapeDataString(returnUrl))}"
                : returnUrl ?? "/");
        }

        // At this point something has gone very wrong, sign out.
        await HttpContext.SignOutAsync();
        return Redirect("authentication/login-failure");
    }

    [HttpPost("logout")]
    public Task LogoutAsync() =>
        HttpContext.SignOutAsync();

}
