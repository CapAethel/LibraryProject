using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryProject.Controllers;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using Xunit;

namespace LibraryProject.Tests
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockBookRepository = new Mock<IBookRepository>();
            _controller = new OrdersController(_mockOrderService.Object, _mockBookRepository.Object);

            // Mock user claims
            var userClaims = new List<Claim>
            {
                new Claim("UserId", "1"), // Assume a user ID of 1
                new Claim(ClaimTypes.Name, "TestUser")
            };
            var identity = new ClaimsIdentity(userClaims, "TestAuthentication");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { OrderId = 1, BookId = 1, Quantity = 2, OrderStatus = "Pending" },
                new Order { OrderId = 2, BookId = 2, Quantity = 3, OrderStatus =    "Pending"}
            };
            _mockOrderService.Setup(service => service.GetAllOrdersAsync())
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.Index() as ViewResult;
            var model = result?.Model as IEnumerable<Order>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ReturnsNotFound_ForInvalidId()
        {
            // Arrange
            _mockOrderService.Setup(service => service.GetOrderByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _controller.Details(null) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithNewOrder()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Book>());

            // Act
            var result = await _controller.Create() as ViewResult;
            var model = result?.Model as Order;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Pending", model.OrderStatus);
            Assert.Equal(DateTime.UtcNow.Date, model.OrderDate.Date);
            Assert.Equal(DateTime.UtcNow.AddDays(21).Date, model.ReturnDate.Date);
        }

        [Fact]
        public async Task Create_Post_CreatesOrderAndRedirectsToCart()
        {
            // Arrange
            var order = new Order { OrderId = 1, BookId = 1, Quantity = 1, OrderStatus = "Pending" };
            _mockOrderService.Setup(service => service.CreateOrderAsync(order))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(order) as RedirectToActionResult;

            // Assert
            _mockOrderService.Verify(service => service.CreateOrderAsync(order), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Cart", result.ActionName);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewWithOrder()
        {
            // Arrange
            var order = new Order { OrderId = 1, BookId = 1, Quantity = 2, OrderStatus = "Pending" };
            _mockOrderService.Setup(service => service.GetOrderByIdAsync(1))
                .ReturnsAsync(order);
            _mockBookRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Book>());

            // Act
            var result = await _controller.Edit(1) as ViewResult;
            var model = result?.Model as Order;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(1, model.OrderId);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DeletesOrderAndRedirectsToCart()
        {
            // Arrange
            _mockOrderService.Setup(service => service.DeleteOrderAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            _mockOrderService.Verify(service => service.DeleteOrderAsync(1), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Cart", result.ActionName);
        }

        [Fact]
        public async Task BookDetails_ReturnsNotFound_ForInvalidBookId()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _controller.BookDetails(null) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Cart_ReturnsViewWithUserOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { OrderId = 1, BookId = 1, Quantity = 1 , OrderStatus = "Pending"},
                new Order { OrderId = 2, BookId = 2, Quantity = 2 , OrderStatus = "Pending"}
            };
            _mockOrderService.Setup(service => service.GetOrdersByUserIdAsync(1))
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.Cart() as ViewResult;
            var model = result?.Model as IEnumerable<Order>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(2, model.Count());
        }
    }
}
