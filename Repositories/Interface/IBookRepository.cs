using LibraryProject.Models;

namespace LibraryProject.Repositories.Interface
{
    public interface IBookRepository
    {
        IQueryable<Book> GetAll();
        Task<Book> GetByIdAsync(int id);
        Task CreateAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(Book book);
        Task<bool> ExistsAsync(int id);
    }
}
