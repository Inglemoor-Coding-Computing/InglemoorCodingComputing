using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace InglemoorCodingComputing.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IUserAuthService _userAuthService;

        public LoginModel(IUserAuthService userAuthService)
        {
            _userAuthService = userAuthService;
        }

        [TempData]
        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public LoginRequest Input { get; set; } = new();

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _userAuthService.AuthenticateAsync(Input.StudentNumber.ToString(), Input.Password);
                if (result is null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.Name, Input.StudentNumber.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()),
                };
                if (result.IsAdmin)
                    claims.Add(new(ClaimTypes.Role, "Admin"));
                if (!result.Verified)
                    claims.Add(new(ClaimTypes.Role, "UnverifiedEmail"));
                ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(claimsIdentity));
                return LocalRedirect(returnUrl);
            }
            return Page();
        }
    }
}
