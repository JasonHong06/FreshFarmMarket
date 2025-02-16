using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.ViewModels;
using FreshFarmMarket.Model;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using FreshFarmMarket.Helpers;
using System.Text.RegularExpressions;
using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace FreshFarmMarket.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _emailSender = emailSender;
            Model = new RegisterViewModel();
        }

        [BindProperty]
        public RegisterViewModel Model { get; set; }

        public void OnGet() => Model = new RegisterViewModel();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (!IsValidPassword(Model.Password))
            {
                ModelState.AddModelError("Password", "Password must be at least 12 characters, include uppercase, lowercase, a number, and a special character.");
                return Page();
            }

            // ✅ Validate reCAPTCHA token
            var isHuman = await VerifyReCaptcha(Request.Form["g-recaptcha-response"]);
            if (!isHuman)
            {
                ModelState.AddModelError(string.Empty, "reCAPTCHA validation failed. Please try again.");
                return Page();
            }

            var existingUser = await _userManager.FindByEmailAsync(Model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return Page();
            }

            // ✅ Validate Credit Card Number
            if (!IsValidCreditCard(Model.CreditCardNo))
            {
                ModelState.AddModelError("CreditCardNo", "Invalid credit card number.");
                return Page();
            }

            string encryptedCreditCardNo = EncryptCreditCard(Model.CreditCardNo);
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile_pics");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = string.Empty;
            if (Model.ProfilePhoto != null)
            {
                // ✅ File Upload Security Check
                if (!ValidateImage(Model.ProfilePhoto.FileName))
                {
                    ModelState.AddModelError("ProfilePhoto", "Invalid file type. Only JPG images are allowed.");
                    return Page();
                }

                fileName = $"{Guid.NewGuid()}_{Path.GetFileName(Model.ProfilePhoto.FileName).Replace(" ", "_")}";
                string filePath = Path.Combine(uploadsFolder, fileName);
                using var fileStream = new FileStream(filePath, FileMode.Create);
                await Model.ProfilePhoto.CopyToAsync(fileStream);
            }

            var user = new ApplicationUser
            {
                FullName = Model.FullName,
                Email = Model.Email,
                UserName = Model.Email,
                Gender = Model.Gender,
                MobileNo = Model.MobileNo,
                DeliveryAddress = Model.DeliveryAddress,
                EncryptedCreditCardNo = encryptedCreditCardNo,
                ProfilePhotoPath = fileName,
                AboutMe = Model.AboutMe ?? "",
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, Model.Password);
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Page("/ConfirmEmail", null, new { userId = user.Id, token = token }, Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Click <a href='{confirmationLink}'>here</a> to confirm.");

                // ✅ Log Account Creation
                AuditLogger.LogAccountCreation(user.Id);

                return RedirectToPage("/CheckEmail");
            }

            return Page();
        }

        private async Task<bool> VerifyReCaptcha(string token)
        {
            using var client = new HttpClient();
            var secretKey = "6LdXb9UqAAAAAPmi6JEOAAlN9E2FvvKs8N52h_5m";
            var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}", null);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic result = JObject.Parse(jsonResponse);
            return result.success == "true" && result.score >= 0.5;
        }

        private string EncryptCreditCard(string creditCardNo) => EncryptionHelper.Encrypt(creditCardNo);

        private bool IsValidCreditCard(string creditCardNo) => Regex.IsMatch(creditCardNo, @"^\d{13,19}$");

        private bool ValidateImage(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension == ".jpg" || extension == ".jpeg";
        }

        // ✅ Server-Side Password Validation
        private bool IsValidPassword(string password)
        {
            return password.Length >= 12 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => !char.IsLetterOrDigit(ch));
        }
    }
}
