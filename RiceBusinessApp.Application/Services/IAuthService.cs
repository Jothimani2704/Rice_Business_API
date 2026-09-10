using System.IO;
using System.Threading.Tasks;
using RiceBusinessApp.Application.DTOs.Auth;

namespace RiceBusinessApp.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<UserResponseDto?> GetCurrentUserAsync(int userId);
        Task<UserResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request);
        Task<UserResponseDto> UploadProfileImageAsync(int userId, Stream fileStream, string fileName);
    }
}
