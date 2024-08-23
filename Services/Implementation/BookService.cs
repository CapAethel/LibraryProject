using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interface;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IQueryable<Book>> GetBooksAsync(string sortOrder, string searchString, string searchCategory, string searchAuthor)
        {
            // Retrieve the books as a List first
            var books = _bookRepository.GetAll();

            bool isFiltered = false;
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString));
                isFiltered = true;
            }
            if (!String.IsNullOrEmpty(searchCategory))
            {
                books = books.Where(b => b.Category.CategoryName.Contains(searchCategory));
                isFiltered = true;
            }
            if (!String.IsNullOrEmpty(searchAuthor))
            {
                books = books.Where(b => b.Author.Contains(searchAuthor));
                isFiltered = true;
            }

            // Sorting logic
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
            books = books
                .OrderBy(b => b.Quantity > 0 ? 0 : 1)
                .ThenBy(b => sortOrder == "title_desc" ? b.Title : b.Author);  // Combine with sorting
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
