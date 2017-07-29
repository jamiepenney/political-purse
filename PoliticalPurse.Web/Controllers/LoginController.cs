using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using PoliticalPurse.Web.Models;
using PoliticalPurse.Web.Services;

namespace PoliticalPurse.Web.Controllers
{
    [Route("user")]
    public class LoginController : Controller
    {
        private const string AuthenticationScheme = "PPCookieMiddleware";
        private readonly UserService _userService;

        public LoginController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("sign_in")]
        public async Task<IActionResult> SignIn()
        {
            var result = await HttpContext.Authentication.AuthenticateAsync(AuthenticationScheme);
            if (result?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Admin");
            }

            return View(new SignInViewModel());
        }

        [HttpPost("sign_in")]
        public async Task<IActionResult> DoSignIn(SignInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("SignIn", new SignInViewModel(model.Email) { Failed = true });
            }

            var user = await _userService.CheckUser(model.Email, model.Password);
            if (user == null)
            {
                //_logger.LogInformation("Login failed for '{model.Email}'");
                ModelState.AddModelError("", "Email or password was incorrect");
                return View("SignIn", new SignInViewModel(model.Email) { Failed = true });
            }
            //_logger.LogInformation("Login succeeded for '{model.Email}'");

            var principal = GetClaimsPrincipal(user);
            await HttpContext.Authentication.SignInAsync(
                AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                });
            return RedirectToAction("Index", "Admin");
        }

        private ClaimsPrincipal GetClaimsPrincipal(User user)
        {
            var principal = new ClaimsPrincipal();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Sid, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Email, user.Email),
            };
            principal.AddIdentity(new ClaimsIdentity(claims, "local"));
            return principal;
        }

        [HttpGet("sign_out")]
        public async Task<IActionResult> Signout()
        {
            await HttpContext.Authentication.SignOutAsync(AuthenticationScheme);

            return RedirectToAction("SignIn");
        }
    }

    public class SignInViewModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }
        public bool Failed { get; set; }

        public SignInViewModel() { }
        public SignInViewModel(string email)
        {
            Email = email;
        }
    }
}
