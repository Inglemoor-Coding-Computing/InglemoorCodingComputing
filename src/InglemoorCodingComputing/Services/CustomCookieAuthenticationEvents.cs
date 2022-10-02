using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly IUserAuthService _userAuthService;

    public CustomCookieAuthenticationEvents(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var userPrincipal = context.Principal;

        // Look for the LastChanged claim.
        var timeStamp = userPrincipal?.Claims.Where(x => x.Type == "TimeStamp").FirstOrDefault()?.Value;
        var id = userPrincipal?.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
        if (Guid.TryParse(id, out var guid) &&
            (!DateTime.TryParse(timeStamp, out var time) || 
             await _userAuthService.TryReadUserAsync(guid) is not UserAuth auth ||
             (auth.SecurityTimeStamp is DateTime ts && time < ts)))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    }
}