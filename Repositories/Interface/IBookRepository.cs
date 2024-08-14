using LibraryProject.Models;

namespace LibraryProject.Repositories.Interface
{
    public interface IBookRepository : IRepository<Book>
    {
        // Define additional methods specific to the Book entity if needed
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author);
        Task<IEnumerable<Book>> GetBooksByCategoryAsync(string category);
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
        public IQueryable<Book> GetAll();
        Task<bool> ExistsAsync(int id);
    }
}
