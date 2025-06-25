using CareerCrafter.Models;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task RegisterUserAsync(User user);
        Task<bool> UserExistsAsync(string email);
    }
}
