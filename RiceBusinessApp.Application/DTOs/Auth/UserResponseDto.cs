namespace RiceBusinessApp.Application.DTOs.Auth
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
