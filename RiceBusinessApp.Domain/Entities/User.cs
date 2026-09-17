using System;

namespace RiceBusinessApp.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string StoreName { get; set; } = "Vellore Rice Mart";
        public string? ProfileImageUrl { get; set; }
        public string Role { get; set; } = "Admin";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
    }
}
