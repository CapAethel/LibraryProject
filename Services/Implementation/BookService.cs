using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryProject.Services.Implementation
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(string sortOrder, string searchString, string searchCategory, string searchAuthor)
        {
            var books = await _bookRepository.GetAllAsync();

            bool isFiltered = false;
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString)).ToList();
                isFiltered = true;
            }
            if (!string.IsNullOrEmpty(searchCategory))
            {
                books = books.Where(b => b.Category.CategoryName.Contains(searchCategory)).ToList();
                isFiltered = true;
            }
            if (!string.IsNullOrEmpty(searchAuthor))
            {
                books = books.Where(b => b.Author.Contains(searchAuthor)).ToList();
                isFiltered = true;
            }

            // Sorting logic
            switch (sortOrder)
            {
                case "title_desc":
                    books = books.OrderByDescending(b => b.Title).ToList();
                    break;
                case "Author":
                    books = books.OrderBy(b => b.Author).ToList();
                    break;
                case "author_desc":
                    books = books.OrderByDescending(b => b.Author).ToList();
                    break;
                case "Category":
                    books = books.OrderBy(b => b.Category.CategoryName).ToList();
                    break;
                case "category_desc":
                    books = books.OrderByDescending(b => b.Category.CategoryName).ToList();
                    break;
                default:
                    books = books.OrderBy(b => b.Title).ToList();
                    break;
            }
            books = books
                .OrderBy(b => b.Quantity > 0 ? 0 : 1)
                .ThenBy(b => sortOrder == "title_desc" ? b.Title : b.Author).ToList();  // Combine with sorting
            return books;
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

        public async Task DeleteBookAsync(Book book)
        {
            await _bookRepository.DeleteAsync(book);
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _bookRepository.ExistsAsync(id);
        }
    }
}
