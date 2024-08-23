using LibraryProject.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryProject.Services.Interface
{
    public interface IAccountService
    {
        Task<UserEditViewModel> GetEditViewModelAsync(int userId);
        Task<bool> UpdateUserAsync(UserEditViewModel viewModel);
        Task<bool> RegisterUserAsync(User user);
        Task<ClaimsPrincipal> LoginAsync(string identifier, string password);
        Task LogOutAsync();
    }
}
