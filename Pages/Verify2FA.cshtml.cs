using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using FreshFarmMarket.Model;

namespace FreshFarmMarket.Pages
{
    public class Verify2FAModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public Verify2FAModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public string VerificationCode { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(VerificationCode, false, false);
            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError("", "Invalid authentication code.");
            return Page();
        }
    }
}
