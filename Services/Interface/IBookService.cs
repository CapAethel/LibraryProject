using LibraryProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryProject.Services.Interface
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetBooksAsync(string sortOrder, string searchString, string searchCategory, string searchAuthor);
        Task<Book> GetBookByIdAsync(int id);
        Task CreateBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(Book book);
        Task<bool> BookExistsAsync(int id);
    }
}
