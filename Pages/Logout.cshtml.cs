using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Model;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FreshFarmMarket.Helpers;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            user.CurrentSessionId = null;  // ✅ Clear session in DB
            await _userManager.UpdateAsync(user);
        }

        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();  // ✅ Clear session in HttpContext

        return RedirectToPage("/Login");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            AuditLogger.LogAction(user.Email, "User Logged Out", "User manually logged out");
        }

        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();  // ✅ Clear session in HttpContext

        return RedirectToPage("/Login");
    }
}
