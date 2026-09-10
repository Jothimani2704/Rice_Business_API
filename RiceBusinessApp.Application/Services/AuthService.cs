using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RiceBusinessApp.Application.DTOs.Auth;
using RiceBusinessApp.Application.Interfaces;
using RiceBusinessApp.Domain.Entities;
using BCrypt.Net;
using System.IO;

namespace RiceBusinessApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password");
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    StoreName = user.StoreName,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Role = user.Role
                }
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                throw new Exception("Username already exists");
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(user);

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    StoreName = user.StoreName,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Role = user.Role
                }
            };
        }

        public async Task<UserResponseDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return null;

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                StoreName = user.StoreName,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role
            };
        }

        public async Task<UserResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // If changing username, check if it already exists
            if (user.Username != request.Username)
            {
                var existingUser = await _userRepository.GetUserByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    throw new Exception("Username already exists");
                }
                user.Username = request.Username;
            }

            user.StoreName = request.StoreName;

            // Password update
            if (!string.IsNullOrEmpty(request.CurrentPassword) && !string.IsNullOrEmpty(request.NewPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    throw new Exception("Incorrect current password");
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            await _userRepository.UpdateUserAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                StoreName = user.StoreName,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role
            };
        }

        public async Task<UserResponseDto> UploadProfileImageAsync(int userId, Stream fileStream, string fileName)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (fileStream == null || fileStream.Length == 0)
            {
                throw new Exception("No image uploaded");
            }

            // Generate a unique file name
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            
            // Assuming the application runs from the root folder (RiceBusinessApp.Api)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            // Update user with the relative path
            var imageUrl = $"/uploads/profiles/{uniqueFileName}";
            user.ProfileImageUrl = imageUrl;
            
            await _userRepository.UpdateUserAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                StoreName = user.StoreName,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
