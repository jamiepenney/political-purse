using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PoliticalPurse.Web.Models;
using PoliticalPurse.Web.Services;


namespace PoliticalPurse.Web.Controllers
{
    [Route("user")]
    public class LoginController : Controller
    {
        private readonly UserService _userService;

        private readonly ILogger<LoginController> _logger;

        public LoginController(UserService userService, ILogger<LoginController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("sign_in")]
        public async Task<IActionResult> SignIn()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result?.Succeeded == true)
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
                _logger.LogWarning($"Login failed for '{model.Email}'");
                ModelState.AddModelError("", "Email or password was incorrect");
                return View("SignIn", new SignInViewModel(model.Email) { Failed = true });
            }
            _logger.LogWarning($"Login succeeded for '{model.Email}'");

            var principal = GetClaimsPrincipal(user);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
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
            principal.AddIdentity(new ClaimsIdentity(claims, "Basic"));
            return principal;
        }

        [HttpGet("sign_out")]
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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
