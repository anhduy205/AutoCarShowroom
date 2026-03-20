using System.Security.Claims;
using AutoCarShowroom.Models;
using AutoCarShowroom.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutoCarShowroom.Controllers
{
    public class AccountController : Controller
    {
        private readonly AdminAccountOptions _adminAccount;

        public AccountController(IOptions<AdminAccountOptions> adminAccountOptions)
        {
            _adminAccount = adminAccountOptions.Value;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Cars");
            }

            return View("~/Login.cshtml", new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Login.cshtml", model);
            }

            var isValidUsername = string.Equals(model.Username, _adminAccount.Username, StringComparison.OrdinalIgnoreCase);
            var isValidPassword = string.Equals(model.Password, _adminAccount.Password, StringComparison.Ordinal);

            if (!isValidUsername || !isValidPassword)
            {
                ModelState.AddModelError(string.Empty, "Tai khoan hoac mat khau khong dung.");
                return View("~/Login.cshtml", model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, _adminAccount.Username),
                new(ClaimTypes.Name, _adminAccount.DisplayName),
                new(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 : 8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["SuccessMessage"] = "Dang nhap thanh cong.";

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Cars");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Dang xuat thanh cong.";
            return RedirectToAction("Index", "Home");
        }
    }
}
