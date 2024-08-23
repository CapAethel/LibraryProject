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
using Humanizer;
using static NuGet.Packaging.PackagingConstants;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Buffers.Text;
using System.Runtime.Intrinsics.X86;

namespace LibraryProject.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Orders.Include(o => o.Book).Include(o => o.user);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Book)
                .Include(o => o.user)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            var order = new Order
            {
                OrderStatus = "Pending", // Explicitly setting it here
                OrderDate = DateTime.UtcNow,
                ReturnDate = DateTime.UtcNow.AddDays(21)
            };
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title");
            return View(order);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,BookId,Quantity,OrderStatus,OrderDate,ReturnDate")] Order order)
        {
            if (!UserIsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the currently logged-in user's ID from claims
            var userId = User.FindFirstValue("UserId");

            // Convert userId to integer (assuming it is stored as int in the database)
            order.UserId = int.Parse(userId);

            if (ModelState.IsValid)
            {
                // Find the book that is being ordered
                var book = await _context.Books.FindAsync(order.BookId);

                if (book == null)
                {
                    ModelState.AddModelError("BookId", "Book not found.");
                    PopulateBookSelectList();
                    return View(order);
                }

                // Check if the requested quantity is available
                if (order.Quantity > book.Quantity)
                {
                    ModelState.AddModelError("Quantity", "The quantity requested exceeds the available stock.");
                    PopulateBookSelectList();
                    return View(order);
                }

                // Deduct the order quantity from the book's stock
                book.Quantity -= order.Quantity;

                // Add the order and update the book's stock in the database
                _context.Update(book);
                _context.Add(order);
                await _context.SaveChangesAsync();

                return RedirectToAction("Cart", "Orders");
            }

            PopulateBookSelectList();
            return View(order);
        }

        private void PopulateBookSelectList()
        {
            // Retrieve books and order them, with out-of-stock books at the end
            var books = _context.Books
                .OrderBy(b => b.Quantity > 0 ? 0 : 1) // Order by stock availability, in-stock first
                .ThenBy(b => b.Title) // Order by title for in-stock and out-of-stock separately
                .ToList();

            // Populate the ViewData with books for the dropdown list
            ViewData["BookId"] = new SelectList(books, "Id", "Title");
        }




        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
       .Include(o => o.Book) // Include the related Book entity
       .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", order.BookId);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,BookId,Quantity,OrderStatus,OrderDate,ReturnDate")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            // Retrieve the currently logged-in user's ID from claims
            var userId = User.FindFirstValue("UserId");

            // Convert userId to integer (assuming it is stored as int in the database)
            order.UserId = int.Parse(userId);

      

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Cart));
            }

            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", order.BookId);
            return View(order);
        }


        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Book)
                .Include(o => o.user)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Book)  // Include Book to access its Quantity
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Return the order quantity to the book stock
            if (order.Book != null && order.OrderStatus == "Pending")
            {
                order.Book.Quantity += order.Quantity;
                _context.Update(order.Book);
            }

            // Remove the order
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Cart", "Orders"); // Redirect to the appropriate page
        }


        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        public async Task<IActionResult> BookDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category) // Ensure Category is included for display
            .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        public async Task<IActionResult> Cart()
        {
            // Check if the user is authenticated
            if (!UserIsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the currently logged-in user's ID from claims
            var userId = User.FindFirstValue("UserId");

            // Convert userId to integer (assuming it is stored as int in the database)
            int userIdInt = int.Parse(userId);

            // Fetch orders that belong to the logged-in user
            var orders = _context.Orders
                .Include(o => o.Book)
                .Include(o => o.user)
                .Where(o => o.UserId == userIdInt); // Filter by UserId

            return View(await orders.ToListAsync());
        }


        private bool UserIsAuthenticated()
        {
            // Check if the user is authenticated
            return User.Identity != null && User.Identity.IsAuthenticated;
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = "Approved";
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DenyOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            var book = await _context.Books.FindAsync(order.BookId);
            if (order == null)
            {
                return NotFound();
            }
            book.Quantity += order.Quantity;
            order.OrderStatus = "Denied";
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ReturnOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Book) // Include the related Book entity
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.OrderStatus != "Approved")
            {
                return BadRequest("Only approved orders can be returned.");
            }

            // Update the book's quantity when the order is returned
            if (order.Book != null)
            {
                order.Book.Quantity += order.Quantity;
                _context.Update(order.Book);
            }

            // Update the order status to "Returned"
            order.OrderStatus = "Returned";
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Cart));
        }

    }
}
