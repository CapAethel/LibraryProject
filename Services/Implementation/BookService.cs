using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryProject.Services.Implementation { 
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;

        public BookService(IBookRepository bookRepository, ICategoryRepository categoryRepository)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<PaginatedList<Book>> GetBooksAsync(string sortOrder, string searchString, string searchCategory, string searchAuthor, int pageNumber, int pageSize)
        {
            // Start by including the related Category entity
            var booksQuery = _bookRepository.GetAll().Include(b => b.Category);

            // Filtering logic
            if (!string.IsNullOrEmpty(searchString))
            {
                booksQuery = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Book, Category?>)booksQuery.Where(b => b.Title.Contains(searchString));
            }
            if (!string.IsNullOrEmpty(searchCategory))
            {
                booksQuery = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Book, Category?>)booksQuery.Where(b => b.Category.CategoryName.Contains(searchCategory));
            }
            if (!string.IsNullOrEmpty(searchAuthor))
            {
                booksQuery = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Book, Category?>)booksQuery.Where(b => b.Author.Contains(searchAuthor));
            }

            // Apply sorting
            var sortedBooksQuery = sortOrder switch
            {
                "title_desc" => booksQuery.OrderByDescending(b => b.Title),
                "Author" => booksQuery.OrderBy(b => b.Author),
                "author_desc" => booksQuery.OrderByDescending(b => b.Author),
                "Category" => booksQuery.OrderBy(b => b.Category.CategoryName),
                "category_desc" => booksQuery.OrderByDescending(b => b.Category.CategoryName),
                _ => booksQuery.OrderBy(b => b.Title),
            };

            return await PaginatedList<Book>.CreateAsync(sortedBooksQuery.AsNoTracking(), pageNumber, pageSize);
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _bookRepository.GetByIdAsync(id);
        }

        public async Task CreateBookAsync(Book book)
        {
            await _bookRepository.CreateAsync(book);
        }

        public async Task UpdateBookAsync(Book book)
        {
            await _bookRepository.UpdateAsync(book);
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book != null)
            {
                await _bookRepository.DeleteAsync(book);
            }
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _bookRepository.ExistsAsync(id);
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

    }
}
