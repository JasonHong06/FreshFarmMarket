using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FreshFarmMarket.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Invalid mobile number. Must be 8-15 digits.")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Delivery Address is required.")]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Credit Card Number is required.")]
        public string CreditCardNo { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Profile photo is required.")]
        public IFormFile ProfilePhoto { get; set; }

        public string? ProfilePhotoPath { get; set; }

        [Required(ErrorMessage = "About Me is required.")]
        public string AboutMe { get; set; }
    }
}
