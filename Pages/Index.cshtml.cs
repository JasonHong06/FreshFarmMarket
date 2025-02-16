using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Model;
using FreshFarmMarket.Helpers;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public string DecryptedCreditCard { get; private set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && !string.IsNullOrEmpty(user.EncryptedCreditCardNo))
        {
            try
            {
                DecryptedCreditCard = EncryptionHelper.Decrypt(user.EncryptedCreditCardNo);
            }
            catch
            {
                DecryptedCreditCard = "Error decrypting card data.";  // Prevent app crash
            }
        }
        else
        {
            DecryptedCreditCard = "No credit card info found.";
        }
    }
}
