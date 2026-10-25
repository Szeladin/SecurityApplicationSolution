using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SecurityApplication.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        public bool IsBlocked { get; set; }

        public bool MustChangePassword { get; set; }

        public DateTime? OneTimePasswordExpiresAt { get; set; }

        public DateTime? PasswordChangedAt { get; set; }
    }
}
