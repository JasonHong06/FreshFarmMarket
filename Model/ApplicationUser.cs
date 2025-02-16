using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace FreshFarmMarket.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string MobileNo { get; set; }
        public string DeliveryAddress { get; set; }
        public string EncryptedCreditCardNo { get; set; }
        public string ProfilePhotoPath { get; set; }
        public string AboutMe { get; set; }
        public string CurrentSessionId { get; set; }

        // ✅ Track last password change date
        public DateTime LastPasswordChangeDate { get; set; } = DateTime.UtcNow;

        // ✅ Store previous hashed passwords (to prevent reuse)
        public List<string> PreviousPasswords { get; set; } = new List<string>();
    }
}
