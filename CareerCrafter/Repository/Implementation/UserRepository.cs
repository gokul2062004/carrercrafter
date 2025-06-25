using CareerCrafter.Data;
using CareerCrafter.Models;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CareerCrafter.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch user by email.", ex);
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch user by ID.", ex);
            }
        }

        public async Task RegisterUserAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to register user.", ex);
            }
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to check if user exists.", ex);
            }
        }
    }
}
