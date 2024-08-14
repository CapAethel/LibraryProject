using LibraryProject.Data;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryProject.Services.Implementation
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICatetoryRepository _categoryRepository;

        public BookService(IBookRepository bookRepository, ICatetoryRepository categoryRepository)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<PaginatedList<Book>> GetBooksAsync(string sortOrder, string searchString, string searchCategory, string searchAuthor, int pageNumber, int pageSize)
        {
            var books = _bookRepository.GetAll();

            // Filtering
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));
            }
            if (!string.IsNullOrEmpty(searchCategory))
            {
                books = books.Where(b => b.Category.CategoryName.Contains(searchCategory));
            }
            if (!string.IsNullOrEmpty(searchAuthor))
            {
                books = books.Where(b => b.Author.Contains(searchAuthor));
            }

            // Sorting
            switch (sortOrder)
            {
                case "title_desc":
                    books = books.OrderByDescending(b => b.Title);
                    break;
                case "Author":
                    books = books.OrderBy(b => b.Author);
                    break;
                case "author_desc":
                    books = books.OrderByDescending(b => b.Author);
                    break;
                case "Category":
                    books = books.OrderBy(b => b.Category.CategoryName);
                    break;
                case "category_desc":
                    books = books.OrderByDescending(b => b.Category.CategoryName);
                    break;
                default:
                    books = books.OrderBy(b => b.Title);
                    break;
            }

            return await PaginatedList<Book>.CreateAsync(books.AsNoTracking(), pageNumber, pageSize);
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _bookRepository.GetByIdAsync(id);
        }

        public async Task AddBookAsync(Book book)
        {
            await _bookRepository.CreateAsync(book);
        }

        public async Task UpdateBookAsync(Book book)
        {
            await _bookRepository.UpdateAsync(book);
        }

        public async Task DeleteBookAsync(int id)
        {
            await _bookRepository.DeleteAsync(await _bookRepository.GetByIdAsync(id));
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
