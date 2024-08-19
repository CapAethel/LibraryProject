using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interface;
using LibraryProject.Services.Interface;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProject.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task RegisterUserAsync(User user)
        {
            user.Password = HashPassword(user.Password);
            user.RoleId = 1; // Assuming 1 is the 'user' role ID
            await _userRepository.CreateAsync(user);
        }

        public async Task<User> ValidateUserAsync(string email, string password)
        {
            string hashedPassword = HashPassword(password);
            return await _userRepository.GetByEmailAndPasswordAsync(email, hashedPassword);
        }

        public string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
