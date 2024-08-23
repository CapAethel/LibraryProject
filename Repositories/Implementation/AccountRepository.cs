using LibraryProject.Data;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryProject.Repositories.Implementation
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetUserByEmailOrNameAsync(string identifier)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == identifier || u.Name == identifier);
        }

        public async Task<User> GetUserByEmailOrNameAndPasswordAsync(string identifier, string hashedPassword)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => (u.Email == identifier || u.Name == identifier) && u.Password == hashedPassword);
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }

        public async Task<bool> EmailOrNameExistsAsync(string email, string name, int excludeUserId = 0)
        {
            return await _context.Users
                .AnyAsync(u => (u.Email == email || u.Name == name) && u.UserId != excludeUserId);
        }

        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
