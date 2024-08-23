using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryProject.Data;
using LibraryProject.Models;
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
        public async Task<IActionResult> Index(string sortOrder, string searchString, string searchCategory, string searchAuthor, int? pageNumber)
        {
            if (!UserIsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

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

            // Order by stock availability (in-stock first, out-of-stock last)
            books = books.OrderBy(b => b.Quantity > 0 ? 0 : 1).ThenBy(b => b.Title);

            // Get the current user's role ID
            var userRoleId = GetUserRoleId();
            ViewData["UserRoleId"] = userRoleId;

            // Pagination logic
            int pageSize = userRoleId == 2 ? 10 : 8; // 10 for admin, 8 for user

            return View(await PaginatedList<Book>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize));
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
        // GET: Books/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
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

            // Re-populate dropdown list if model state is invalid
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", book.CategoryId);
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

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
    }
}
