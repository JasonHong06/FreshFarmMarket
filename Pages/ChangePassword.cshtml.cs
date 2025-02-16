using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FreshFarmMarket.Helpers;

namespace FreshFarmMarket.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private const int PasswordHistoryLimit = 2;  // Prevent last 2 passwords from being reused
        private static readonly TimeSpan MinPasswordAge = TimeSpan.FromMinutes(5);  // Prevent changing too soon
        private static readonly TimeSpan MaxPasswordAge = TimeSpan.FromDays(90);  // Require change after 90 days

        public ChangePasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [BindProperty]
        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            // ✅ Enforce minimum password age
            if (DateTime.UtcNow - user.LastPasswordChangeDate < MinPasswordAge)
            {
                ModelState.AddModelError("", $"You must wait {MinPasswordAge.TotalMinutes} minutes before changing your password again.");
                return Page();
            }

            // ✅ Prevent password reuse
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            if (user.PreviousPasswords.Any(oldPass => passwordHasher.VerifyHashedPassword(user, oldPass, NewPassword) == PasswordVerificationResult.Success))
            {
                ModelState.AddModelError("", "You cannot reuse your last 2 passwords.");
                return Page();
            }

            // ✅ Check if new password is in previous passwords
            var previousPasswords = await _userManager.GetLoginsAsync(user);
            if (previousPasswords.Any(h => h.ProviderKey == NewPassword))
            {
                ModelState.AddModelError("", "You cannot reuse an old password.");
                return Page();
            }

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (result.Succeeded)
            {
                // ✅ Store new password in history
                user.PreviousPasswords.Add(passwordHasher.HashPassword(user, NewPassword));
                if (user.PreviousPasswords.Count > PasswordHistoryLimit)
                {
                    user.PreviousPasswords.RemoveAt(0);  // Remove oldest password
                }

                user.LastPasswordChangeDate = DateTime.UtcNow;  // ✅ Update last change date
                await _userManager.UpdateAsync(user);

                await _signInManager.RefreshSignInAsync(user);

                AuditLogger.LogPasswordChange(user.Id, "Password successfully changed.");

                return RedirectToPage("/Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}

