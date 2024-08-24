using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryProject.Controllers;
using LibraryProject.Models;
using LibraryProject.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibraryProject.Tests
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoriesController(_mockCategoryService.Object);
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Fiction" },
                new Category { CategoryId = 2, CategoryName = "Non-Fiction" }
            };
            _mockCategoryService.Setup(service => service.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.Index() as ViewResult;
            var model = result?.Model as IEnumerable<Category>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ReturnsNotFound_ForInvalidId()
        {
            // Arrange
            _mockCategoryService.Setup(service => service.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _controller.Details(null) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }


        [Fact]
        public async Task Create_Post_CreatesCategory_AndRedirectsToIndex()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "Science" };
            _mockCategoryService.Setup(service => service.CreateCategoryAsync(category))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(category) as RedirectToActionResult;

            // Assert
            _mockCategoryService.Verify(service => service.CreateCategoryAsync(category), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(nameof(CategoriesController.Index), result.ActionName);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewWithCategory()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "Fantasy" };
            _mockCategoryService.Setup(service => service.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);

            // Act
            var result = await _controller.Edit(1) as ViewResult;
            var model = result?.Model as Category;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(1, model.CategoryId);
            Assert.Equal("Fantasy", model.CategoryName);
        }

        [Fact]
        public async Task Edit_Post_UpdatesCategory_AndRedirectsToIndex()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "History" };
            _mockCategoryService.Setup(service => service.UpdateCategoryAsync(category))
                .Returns(Task.CompletedTask);
            _mockCategoryService.Setup(service => service.CategoryExistsAsync(category.CategoryId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Edit(category.CategoryId, category) as RedirectToActionResult;

            // Assert
            _mockCategoryService.Verify(service => service.UpdateCategoryAsync(category), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(nameof(CategoriesController.Index), result.ActionName);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            // Act
            var result = await _controller.Edit(2, new Category { CategoryId = 1 }) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewWithCategory()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "Biography" };
            _mockCategoryService.Setup(service => service.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);

            // Act
            var result = await _controller.Delete(1) as ViewResult;
            var model = result?.Model as Category;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(1, model.CategoryId);
        }

        [Fact]
        public async Task DeleteConfirmed_Post_DeletesCategory_AndRedirectsToIndex()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "Romance" };
            _mockCategoryService.Setup(service => service.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);
            _mockCategoryService.Setup(service => service.DeleteCategoryAsync(category))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            _mockCategoryService.Verify(service => service.DeleteCategoryAsync(category), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(nameof(CategoriesController.Index), result.ActionName);
        }
    }
}
