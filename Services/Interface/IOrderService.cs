using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryProject.Models;

namespace LibraryProject.Services.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task CreateOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(int id);
        Task ApproveOrderAsync(int id);
        Task DenyOrderAsync(int id);
        Task ReturnOrderAsync(int id);
    }
}
