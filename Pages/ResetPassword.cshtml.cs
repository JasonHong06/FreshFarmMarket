using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Model;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FreshFarmMarket.Helpers;

namespace FreshFarmMarket.Pages
{
    public class ResetPasswordModel : PageModel
    {
        public required UserManager<ApplicationUser> _userManager { get; init; }


        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }


        [BindProperty]
        public string Token { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool ResetSuccessful { get; set; } = false;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null) return Page();  // Don't reveal if email exists

            var result = await _userManager.ResetPasswordAsync(user, Token, Password);
            if (result.Succeeded)
            {
                ResetSuccessful = true;

                // Log password reset success using AuditLogger
                AuditLogger.LogPasswordResetSuccess(user.Id);  // Static method call for logging

                return RedirectToPage("/Login");  // Redirect to Login after successful reset
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
