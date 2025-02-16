using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace FreshFarmMarket.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string EncryptedCreditCardNo { get; set; } = string.Empty;
        public string ProfilePhotoPath { get; set; } = string.Empty;
        public string AboutMe { get; set; } = string.Empty;
        public string? CurrentSessionId { get; set; }

        // ✅ Track last password change date
        public DateTime LastPasswordChangeDate { get; set; } = DateTime.UtcNow;

        // ✅ Store previous hashed passwords (to prevent reuse)
        public List<string> PreviousPasswords { get; set; } = new List<string>();
    }
}
