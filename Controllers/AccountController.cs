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
        private readonly AdminAccountOptions _accountOptions;

        public AccountController(IOptions<AdminAccountOptions> adminAccountOptions)
        {
            _accountOptions = adminAccountOptions.Value;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Cars");
            }

            return View(new LoginViewModel
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
                return View(model);
            }

            var account = _accountOptions.GetAccounts().FirstOrDefault(item =>
                string.Equals(item.Username, model.Username, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.Password, model.Password, StringComparison.Ordinal));

            if (account == null)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                return View(model);
            }

            var normalizedRole = InternalAccess.NormalizeRole(account.Role);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Username),
                new(ClaimTypes.Name, string.IsNullOrWhiteSpace(account.DisplayName) ? account.Username : account.DisplayName),
                new(ClaimTypes.Role, normalizedRole)
            };

            if (string.Equals(normalizedRole, InternalAccess.RoleAdmin, StringComparison.OrdinalIgnoreCase) || account.CanAccessRevenue)
            {
                claims.Add(new Claim(InternalAccess.PermissionClaimType, InternalAccess.RevenuePermission));
            }

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

            TempData["SuccessMessage"] = "Đăng nhập nội bộ thành công.";

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Cars");
        }

        [HttpPost]
        [Authorize(Roles = InternalAccess.BackOfficeRoles)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }
    }
}
