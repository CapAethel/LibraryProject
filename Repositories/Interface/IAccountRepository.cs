using LibraryProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryProject.Repositories.Interface
{
    public interface IAccountRepository
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailOrNameAsync(string identifier);
        Task<User> GetUserByEmailOrNameAndPasswordAsync(string identifier, string hashedPassword);
        Task<bool> UserExistsAsync(int id);
        Task<bool> EmailOrNameExistsAsync(string email, string name, int excludeUserId = 0);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task SaveChangesAsync();
    }
}
