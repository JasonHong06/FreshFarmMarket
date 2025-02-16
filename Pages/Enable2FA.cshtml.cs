using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder; // Ensure you have this 'using' directive
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FreshFarmMarket.Model;

namespace FreshFarmMarket.Pages
{
    public class Enable2FAModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public Enable2FAModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public string QRCodeImage { get; set; }

        [BindProperty]
        public string ManualEntryKey { get; set; }

        [BindProperty]
        public string VerificationCode { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            ManualEntryKey = key;
            QRCodeImage = GenerateQRCode($"otpauth://totp/FreshFarmMarket:{user.Email}?secret={key}&issuer=FreshFarmMarket");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Login");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, VerificationCode);
            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid verification code.");
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            return RedirectToPage("/Index");
        }

        private string GenerateQRCode(string data)
        {
            using (QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator())
            {
                using (QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCoder.QRCodeGenerator.ECCLevel.Q))
                {
                    using (QRCoder.PngByteQRCode qrCode = new QRCoder.PngByteQRCode(qrCodeData)) // ? Use PngByteQRCode instead of QRCode
                    {
                        byte[] qrCodeBytes = qrCode.GetGraphic(20);
                        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
                    }
                }
            }
        }


    }
}
