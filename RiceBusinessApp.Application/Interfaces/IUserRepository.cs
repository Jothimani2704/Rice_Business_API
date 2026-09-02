using System.Threading.Tasks;
using RiceBusinessApp.Domain.Entities;

namespace RiceBusinessApp.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task<User> AddUserAsync(User user);
    }
}
