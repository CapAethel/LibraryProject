using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBookRepository _bookRepository;

        public OrderService(IOrderRepository orderRepository, IBookRepository bookRepository)
        {
            _orderRepository = orderRepository;
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _orderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task CreateOrderAsync(Order order)
        {
            var book = await _bookRepository.GetByIdAsync(order.BookId);

            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            if (order.Quantity > book.Quantity)
            {
                throw new InvalidOperationException("The quantity requested exceeds the available stock.");
            }

            book.Quantity -= order.Quantity;
            await _bookRepository.UpdateAsync(book);

            await _orderRepository.AddOrderAsync(order);
        }

        public async Task UpdateOrderAsync(Order updatedOrder)
        {
            var existingOrder = await _orderRepository.GetOrderByIdAsync(updatedOrder.OrderId);

            if (existingOrder == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            var book = await _bookRepository.GetByIdAsync(existingOrder.BookId);

            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            // Calculate the difference in quantity
            int quantityDifference = updatedOrder.Quantity - existingOrder.Quantity;

            // Check if the new quantity exceeds the available stock
            if (quantityDifference > 0 && quantityDifference > book.Quantity)
            {
                throw new InvalidOperationException("The quantity requested exceeds the available stock.");
            }

            // Update the book quantity
            book.Quantity -= quantityDifference;
            await _bookRepository.UpdateAsync(book);

            // Update the order details
            existingOrder.Quantity = updatedOrder.Quantity;
            existingOrder.OrderStatus = updatedOrder.OrderStatus;
            existingOrder.OrderDate = updatedOrder.OrderDate;
            existingOrder.ReturnDate = updatedOrder.ReturnDate;

            try
            {
                await _orderRepository.UpdateOrderAsync(existingOrder);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_orderRepository.OrderExists(updatedOrder.OrderId))
                {
                    throw new KeyNotFoundException("Order not found.");
                }
                else
                {
                    throw;
                }
            }
        }


        public async Task DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.Book != null && order.OrderStatus == "Pending")
            {
                order.Book.Quantity += order.Quantity;
                await _bookRepository.UpdateAsync(order.Book);
            }

            await _orderRepository.DeleteOrderAsync(order);
        }

        public async Task ApproveOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            order.OrderStatus = "Approved";
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task DenyOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            var book = await _bookRepository.GetByIdAsync(order.BookId);
            if (book != null)
            {
                book.Quantity += order.Quantity;
                await _bookRepository.UpdateAsync(book);
            }

            order.OrderStatus = "Denied";
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task ReturnOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.OrderStatus != "Approved")
            {
                throw new InvalidOperationException("Only approved orders can be returned.");
            }

            if (order.Book != null)
            {
                order.Book.Quantity += order.Quantity;
                await _bookRepository.UpdateAsync(order.Book);
            }

            order.OrderStatus = "Returned";
            await _orderRepository.UpdateOrderAsync(order);
        }
    }
}
