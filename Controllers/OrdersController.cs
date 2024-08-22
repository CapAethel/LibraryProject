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
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Cart", "Orders");
            }

            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", order.BookId);
            return View(order);
        }


        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
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


        // Method to check if the order exists


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
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));
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


    }
}
