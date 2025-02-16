using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FreshFarmMarket.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Invalid mobile number. Must be 8-15 digits.")]
        public string MobileNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery Address is required.")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Credit Card Number is required.")]
        public string CreditCardNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Profile photo is required.")]
        public IFormFile? ProfilePhoto { get; set; }

        public string? ProfilePhotoPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "About Me is required.")]
        public string AboutMe { get; set; } = string.Empty;
    }
}
