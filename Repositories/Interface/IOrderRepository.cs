using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryProject.Models;

namespace LibraryProject.Repositories.Interface
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task AddOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(Order order);
        Task SaveChangesAsync();
        bool OrderExists(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    }
}
