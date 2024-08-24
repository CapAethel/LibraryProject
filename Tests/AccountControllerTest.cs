using System.Security.Claims;
using System.Threading.Tasks;
using LibraryProject.Controllers;
using LibraryProject.Models;
using LibraryProject.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NuGet.ContentModel;
using Xunit;

namespace LibraryProject.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _mockAccountService;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockAccountService = new Mock<IAccountService>();
            _controller = new AccountController(_mockAccountService.Object);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var controller = new AccountController(_mockAccountService.Object);

            // Act
            var result = await controller.Edit((int?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            _mockAccountService.Setup(service => service.GetEditViewModelAsync(It.IsAny<int>()))
                .ReturnsAsync((UserEditViewModel)null);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithViewModel()
        {
            // Arrange
            var viewModel = new UserEditViewModel();
            _mockAccountService.Setup(service => service.GetEditViewModelAsync(It.IsAny<int>()))
                .ReturnsAsync(viewModel);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(viewModel, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Edit(new UserEditViewModel());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<UserEditViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewResult_WhenUpdateFails()
        {
            // Arrange
            _mockAccountService.Setup(service => service.UpdateUserAsync(It.IsAny<UserEditViewModel>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Edit(new UserEditViewModel());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task Edit_Post_RedirectsToHomeIndex_WhenUpdateSucceeds()
        {
            // Arrange
            _mockAccountService.Setup(service => service.UpdateUserAsync(It.IsAny<UserEditViewModel>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Edit(new UserEditViewModel());

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public void Register_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Register();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Register_Post_ReturnsViewResult_WhenRegistrationFails()
        {
            // Arrange
            _mockAccountService.Setup(service => service.RegisterUserAsync(It.IsAny<User>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Register(new User());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task Register_Post_RedirectsToLogin_WhenRegistrationSucceeds()
        {
            // Arrange
            _mockAccountService.Setup(service => service.RegisterUserAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(new User());

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public void Login_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Login();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_Post_ReturnsViewResult_WhenLoginFails()
        {
            // Arrange
            _mockAccountService.Setup(service => service.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((ClaimsPrincipal)null);

            // Act
            var result = await _controller.Login("identifier", "password");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task Login_Post_RedirectsToBooksIndex_WhenLoginSucceeds()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal();
            _mockAccountService.Setup(service => service.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(claimsPrincipal);

            // Act
            var result = await _controller.Login("identifier", "password");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Books", redirectResult.ControllerName);
        }

        [Fact]
        public async Task LogOut_Post_RedirectsToLogin()
        {
            // Act
            var result = await _controller.LogOut();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
    }
}
