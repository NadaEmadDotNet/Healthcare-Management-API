using Microsoft.AspNetCore.Identity;

namespace Medication_Reminder_API.Infrastructure
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public int TokenVersion { get; set; } 
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }

    }
}
