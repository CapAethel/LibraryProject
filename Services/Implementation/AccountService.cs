using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProject.Services.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(IAccountRepository accountRepository, IHttpContextAccessor httpContextAccessor)
        {
            _accountRepository = accountRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserEditViewModel> GetEditViewModelAsync(int userId)
        {
            var user = await _accountRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserEditViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email
            };
        }

        public async Task<bool> UpdateUserAsync(UserEditViewModel viewModel)
        {
            var existingUser = await _accountRepository.GetUserByIdAsync(viewModel.UserId);
            if (existingUser == null)
            {
                return false;
            }

            if (await _accountRepository.EmailOrNameExistsAsync(viewModel.Email, viewModel.Name, viewModel.UserId))
            {
                return false; // Email or name already exists
            }

            if (!string.IsNullOrEmpty(viewModel.OldPassword) && !string.IsNullOrEmpty(viewModel.NewPassword))
            {
                if (existingUser.Password != HashPassword(viewModel.OldPassword))
                {
                    return false; // Old password is incorrect
                }

                if (!IsValidPassword(viewModel.NewPassword))
                {
                    return false; // New password does not meet the requirements
                }

                existingUser.Password = HashPassword(viewModel.NewPassword);
            }

            existingUser.Name = viewModel.Name;
            existingUser.Email = viewModel.Email;

            await _accountRepository.UpdateUserAsync(existingUser);
            return true;
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            if (await _accountRepository.EmailOrNameExistsAsync(user.Email, user.Name))
            {
                return false; // Email or name already exists
            }

            user.RoleId = 1;

            if (!IsValidPassword(user.Password))
            {
                return false; // Password does not meet the requirements
            }

            user.Password = HashPassword(user.Password);

            await _accountRepository.CreateUserAsync(user);
            return true;
        }

        public async Task<ClaimsPrincipal> LoginAsync(string identifier, string password)
        {
            string hashedPassword = HashPassword(password);
            var user = await _accountRepository.GetUserByEmailOrNameAndPasswordAsync(identifier, hashedPassword);

            if (user == null)
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            return claimsPrincipal;
        }

        public async Task LogOutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private string HashPassword(string password)
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

        private bool IsValidPassword(string password)
        {
            var hasMinimumLength = password.Length >= 8;
            var hasUpperCase = password.Any(c => char.IsUpper(c));
            var hasLowerCase = password.Any(c => char.IsLower(c));
            var hasDigit = password.Any(c => char.IsDigit(c));
            var hasSpecialChar = password.Any(c => "!@#$%^&*()_+[]{}|;:,.<>?/".Contains(c));

            return hasMinimumLength && hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }
    }
}
