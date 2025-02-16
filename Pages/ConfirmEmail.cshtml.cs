using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Model;
using System.Threading.Tasks;

namespace FreshFarmMarket.Pages
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public bool Success { get; private set; }

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            if (userId == null || token == null)
                return RedirectToPage("/Errors/404");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToPage("/Errors/404");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            Success = result.Succeeded;
            return Page();
        }


    }
}
