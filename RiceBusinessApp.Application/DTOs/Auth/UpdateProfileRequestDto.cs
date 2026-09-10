using System.ComponentModel.DataAnnotations;

namespace RiceBusinessApp.Application.DTOs.Auth
{
    public class UpdateProfileRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string StoreName { get; set; } = string.Empty;

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
