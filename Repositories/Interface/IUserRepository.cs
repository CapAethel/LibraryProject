using LibraryProject.Models;
using System.Threading.Tasks;

namespace LibraryProject.Repositories.Interface
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAndPasswordAsync(string email, string hashedPassword);
    }
}
