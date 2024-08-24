using LibraryProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryProject.Repositories.Interface
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book> GetByIdAsync(int id);
        Task CreateAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(Book book);
        Task<bool> ExistsAsync(int id);
    }
}
