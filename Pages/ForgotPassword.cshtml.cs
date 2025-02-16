using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Model;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FreshFarmMarket.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        public bool EmailSent { get; private set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Email);
            await Task.Delay(TimeSpan.FromSeconds(3)); // Add delay to prevent user enumeration

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Page("/ResetPassword", null, new { token, email = user.Email }, Request.Scheme);
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await _emailSender.SendEmailAsync(user.Email, "Password Reset", $"Click <a href='{resetLink}'>here</a> to reset your password.");
                }
                   
            }

            EmailSent = true; // Always return success, even if the email is invalid
            return Page();
        }
    }
}

