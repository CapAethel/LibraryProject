using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryProject.Data;
using LibraryProject.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LibraryProject.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string sortOrder, string searchString, string searchCategory, string searchAuthor)
        {
            // Sorting parameters and logic
            ViewData["TitleSortParam"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["AuthorSortParam"] = sortOrder == "Author" ? "author_desc" : "Author";
            ViewData["CategorySortParam"] = sortOrder == "Category" ? "category_desc" : "Category";

            var books = from b in _context.Books.Include(b => b.Category)
                        select b;
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            // Filtering logic
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

            // Set the ViewData flag
            ViewData["Filtered"] = isFiltered;

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

            var userRoleId = GetUserRoleId();
            ViewData["UserRoleId"] = userRoleId;

            return View(await books.ToListAsync());
        }




        private int GetUserRoleId()
        {
            var userRoleIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (int.TryParse(userRoleIdClaim, out var roleId))
            {
                return roleId;
            }

            return 1;
        }
        private bool UserIsAuthenticated()
        {
            // Check if the user is authenticated
            return User.Identity != null && User.Identity.IsAuthenticated;
        }


        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Author,CategoryId,BookDescription,PictureUrl,Quantity")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", book.CategoryId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", book.CategoryId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,CategoryId,BookDescription,PictureUrl,Quantity")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", book.CategoryId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order(int bookId)
        {
            if (!UserIsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetUserId();
            var book = await _context.Books.FindAsync(bookId);

            if (book == null)
            {
                return NotFound();
            }

            // Check the user's cart and limit it to 5 items
            var cartCount = await _context.Orders.CountAsync(o => o.UserId == userId);
            if (cartCount >= 5)
            {
                // Optionally show a message to the user
                return RedirectToAction("Index", "Books");
            }

            var order = new Order
            {
                BookId = bookId,
                Quantity = 1, // Default quantity
                UserId = userId,
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Books");
        }

        private int GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return -1; // Or handle as appropriate
        }
        public IQueryable<Book> GetAll()
        {
            return _context.Books.AsQueryable();
        }

        public async Task<PaginatedList<Book>> GetBookAsync(string filterField, string filterCriteria, string filterValue,
                                                        int pageNumber, int pageSize)
        {
            var books = GetAll();
            if (!string.IsNullOrEmpty(filterField))
            {
                switch (filterField)
                {
                    case "Author":
                        if (!string.IsNullOrEmpty(filterCriteria))
                        {
                            string filterValueLower = filterValue.ToLower();
                            books = books.Where(b => b.Author.ToLower().Contains(filterValueLower));
                        }
                        break;
                    case "Title":
                        if (!string.IsNullOrEmpty(filterCriteria))
                        {
                            string filterValueLower = filterValue.ToLower();
                            books = books.Where(b => b.Title.ToLower().Contains(filterValueLower));
                        }
                        break;
                    case "Category":
                        if (!string.IsNullOrEmpty(filterCriteria))
                        {
                            books = books.Where(b => b.Category.CategoryName == filterCriteria);
                        }
                        break;
                }
            }
            return await PaginatedList<Book>.CreateAsync(books, pageNumber, pageSize);
        }
    }
}
