using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services;
using LibraryProject.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IBookRepository _bookRepository;

        public OrdersController(IOrderService orderService, IBookRepository bookRepository)
        {
            _orderService = orderService;
            _bookRepository = bookRepository;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            PopulateBookSelectList();
            var order = new Order
            {
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow,
                ReturnDate = DateTime.UtcNow.AddDays(21)
            };
            return View(order);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,BookId,Quantity,OrderStatus,OrderDate,ReturnDate")] Order order)
        {
            if (!UserIsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue("UserId");
            order.UserId = int.Parse(userId);

            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.CreateOrderAsync(order);
                    return RedirectToAction("Cart", "Orders");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Quantity", ex.Message);
                }
                catch (KeyNotFoundException ex)
                {
                    ModelState.AddModelError("BookId", ex.Message);
                }
            }

            PopulateBookSelectList();
            return View(order);
        }

        private void PopulateBookSelectList()
        {
            var books = _bookRepository.GetAll()
                .OrderBy(b => b.Quantity > 0 ? 0 : 1)
                .ThenBy(b => b.Title)
                .ToList();

            ViewData["BookId"] = new SelectList(books, "Id", "Title");
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            PopulateBookSelectList();
            ViewData["BookId"] = new SelectList(await _bookRepository.GetAll().ToListAsync(), "Id", "Title", order.BookId);
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

            var userId = User.FindFirstValue("UserId");
            order.UserId = int.Parse(userId);

            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.UpdateOrderAsync(order);
                    return RedirectToAction(nameof(Cart));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Quantity", ex.Message);
                }
            }

            ViewData["BookId"] = new SelectList(await _bookRepository.GetAll().ToListAsync(), "Id", "Title", order.BookId);
            return View(order);
        }


        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
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
            try
            {
                await _orderService.DeleteOrderAsync(id);
                return RedirectToAction("Cart", "Orders");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> BookDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        public async Task<IActionResult> Cart()
        {
            if (!UserIsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue("UserId");
            int userIdInt = int.Parse(userId);

            var orders = await _orderService.GetOrdersByUserIdAsync(userIdInt);

            return View(orders);
        }

        private bool UserIsAuthenticated()
        {
            return User.Identity != null && User.Identity.IsAuthenticated;
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            try
            {
                await _orderService.ApproveOrderAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DenyOrder(int id)
        {
            try
            {
                await _orderService.DenyOrderAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReturnOrder(int id)
        {
            try
            {
                await _orderService.ReturnOrderAsync(id);
                return RedirectToAction(nameof(Cart));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
