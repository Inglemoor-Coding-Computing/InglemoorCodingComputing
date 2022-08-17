using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace InglemoorCodingComputing.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IUserAuthService _userAuthService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public RegisterModel(IUserAuthService userAuthService, IUserService userService, IEmailService emailService)
        {
            _userAuthService = userAuthService;
            _userService = userService;
            _emailService = emailService;
        }

        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public RegisterRequest Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var userAuth = await _userAuthService.AddUserAsync(Input.StudentNumber.ToString(), Input.Password);
                if (userAuth is null)
                {
                    ModelState.AddModelError(string.Empty, "Student Number already registered.");
                    return Page();
                }

                // Send an email
                _emailService.Send($"{Input.StudentNumber}@apps.nsd.org", "Confirm email for Inglemoor Coding & Computing Club", $"Hi {Input.FirstName},\nVerify your email/studentid here: http://{Request.Host}/authentication/verify-email?token={userAuth.VerificationToken}\nThank You\n\nIgnore this email if not expected.");

                // Create user
                await _userService.CreateUser(new()
                {
                    Id = userAuth.Id,
                    StudentNumber = Input.StudentNumber,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    GraduationYear = AppUser.GradeLevelToGraduationYear(Input.GradeLevel),
                    CreatedDate = DateTime.UtcNow,
                });

                // Log in
                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.Name, Input.StudentNumber.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userAuth.Id.ToString()),
                    new Claim(ClaimTypes.Role, "UnverifiedEmail"),
                };

                ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(claimsIdentity));
                return LocalRedirect(Url.Content("~/authentication/register-success"));
            }
            return Page();
        }
    }
}
