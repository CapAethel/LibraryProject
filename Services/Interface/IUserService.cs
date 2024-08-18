using LibraryProject.Models;
using System.Threading.Tasks;

namespace LibraryProject.Services.Interface
{
    public interface IUserService
    {
        Task RegisterUserAsync(User user);
        Task<User> ValidateUserAsync(string email, string password);
        string HashPassword(string password);
    }
}
