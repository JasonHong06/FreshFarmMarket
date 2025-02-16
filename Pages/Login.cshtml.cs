using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FreshFarmMarket.Helpers;

namespace FreshFarmMarket.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Password { get; set; }
        public bool LoginFailed { get; set; } = false;
        public bool IsLockedOut { get; set; } = false;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // ✅ Sanitize input to prevent XSS & SQL injection
            Email = SanitizeInput(Email);
            Password = SanitizeInput(Password);

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                AuditLogger.LogAction(Email, "Failed Login Attempt", "Invalid credentials");
                LoginFailed = true;
                return Page();
            }

            // ✅ Check if the account is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                IsLockedOut = true;
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(Email, Password, false, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                string ipAddress = GetClientIp();
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                // ✅ Log login attempt
                AuditLogger.LogAction(user.Email, "User logged in", $"IP: {ipAddress}, Timestamp: {timestamp}");

                return RedirectToPage("/Index");
            }

            // Log failed login attempt
            AuditLogger.LogAction(user.Email, "Failed Login Attempt", "Invalid credentials");

            LoginFailed = true;
            return Page();
        }

        private string GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string SanitizeInput(string input)
        {
            return Regex.Replace(input, @"[^\w\s@.-]", ""); // Removes special characters except @, ., and -
        }
    }
}
