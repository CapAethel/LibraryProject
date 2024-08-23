using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Data;

namespace LibraryProject.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Book)
                .Include(o => o.user)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Book)
                .Include(o => o.user)
                .FirstOrDefaultAsync(m => m.OrderId == id);
        }

        public async Task AddOrderAsync(Order order)
        {
            _context.Add(order);
            await SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Update(order);
            await SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(Order order)
        {
            _context.Orders.Remove(order);
            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Book)
                .Include(o => o.user)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }
    }
}
