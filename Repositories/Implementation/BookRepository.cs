using LibraryProject.Data;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Repositories.Implementation
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }
        public IQueryable<Book> GetAll()
        {
            return _context.Books.AsQueryable();
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Books.AnyAsync(e => e.Id == id);
        }
        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author)
        {
            return await _context.Books
                .Where(b => b.Author.Contains(author))
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(string category)
        {
            return await _context.Books
                .Where(b => b.Category.CategoryName.Contains(category))
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            return await _context.Books
                .Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm))
                .ToListAsync();
        }
    }
}
