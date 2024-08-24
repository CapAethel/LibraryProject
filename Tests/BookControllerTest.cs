using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryProject.Controllers;
using LibraryProject.Models;
using LibraryProject.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using Xunit;

namespace LibraryProject.Tests
{
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new BooksController(_mockBookService.Object, _mockCategoryService.Object);

            // Mock user claims
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "1") // Assume a user role ID of 1
            };
            var identity = new ClaimsIdentity(userClaims, "TestAuthentication");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book 1" },
                new Book { Id = 2, Title = "Book 2" }
            };
            _mockBookService.Setup(service => service.GetBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(books);
            _mockCategoryService.Setup(service => service.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Index(null, null, null, null, null) as ViewResult;
            var model = result?.Model as PaginatedList<Book>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(2, model?.Count);
        }

        [Fact]
        public async Task Create_Post_CreatesBookAndRedirectsToIndex()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "New Book" };
            _mockBookService.Setup(service => service.CreateBookAsync(book))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(book) as RedirectToActionResult;

            // Assert
            _mockBookService.Verify(service => service.CreateBookAsync(book), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewWithBook()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Existing Book" };
            _mockBookService.Setup(service => service.GetBookByIdAsync(1))
                .ReturnsAsync(book);
            _mockCategoryService.Setup(service => service.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Edit(1) as ViewResult;
            var model = result?.Model as Book;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Existing Book", model?.Title);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DeletesBookAndRedirectsToIndex()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Book to Delete" };
            _mockBookService.Setup(service => service.GetBookByIdAsync(1))
                .ReturnsAsync(book);
            _mockBookService.Setup(service => service.DeleteBookAsync(book))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            _mockBookService.Verify(service => service.DeleteBookAsync(book), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }
    }
}
