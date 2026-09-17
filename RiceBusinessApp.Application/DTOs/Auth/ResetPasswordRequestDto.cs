namespace RiceBusinessApp.Application.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
