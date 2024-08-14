using LibraryProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryProject.Services.Interfaces
{
    public interface IBookService
    {
        Task<PaginatedList<Book>> GetBooksAsync(string sortOrder, string searchString, string searchCategory, string searchAuthor, int pageNumber, int pageSize);
        Task<Book> GetBookByIdAsync(int id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int id);
        Task<bool> BookExistsAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }

}
