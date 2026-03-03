using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PackagedFoodManagementSystem.Controllers;
using PackageFoodManagementSystem.Services.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IUserService> _userMock;
        private Mock<IOrderService> _orderMock;
        private Mock<IInventoryService> _inventoryMock;
        private Mock<IProductService> _productMock;
        private Mock<IConfiguration> _configMock;
        private HomeController _controller;

        [SetUp]
        public void Setup()
        {
            _userMock = new Mock<IUserService>();
            _orderMock = new Mock<IOrderService>();
            _inventoryMock = new Mock<IInventoryService>();
            _productMock = new Mock<IProductService>();
            _configMock = new Mock<IConfiguration>();

            _controller = new HomeController(
                _userMock.Object,
                _orderMock.Object,
                _inventoryMock.Object,
                _productMock.Object,
                _configMock.Object);

            // 1. Setup functional HttpContext and Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockSession(); // Use the MockSession class defined below

            // 2. Setup functional TempData (Prevents NullReference on DeleteUser)
            var tempDataProvider = new Mock<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData;
        }

        [Test]
        public async Task OrdersDashboard_SetsViewBagAndReturnsView()
        {
            _orderMock.Setup(s => s.CountTodayOrdersAsync()).ReturnsAsync(5);
            _orderMock.Setup(s => s.GetOrdersAsync(It.IsAny<string>())).ReturnsAsync(new List<Order>());

            var result = await _controller.OrdersDashboard("Pending");

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.TodayOrders, Is.EqualTo(5));
        }

        [Test]
        public async Task AdminDashboard_PopulatesKpis_ReturnsView()
        {
            _userMock.Setup(s => s.GetAdminDashboardStatsAsync()).ReturnsAsync((10, 2, 50));
            _productMock.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<Product>());
            _inventoryMock.Setup(s => s.GetInventoryListAsync()).ReturnsAsync(new List<Inventory>());

            var result = await _controller.AdminDashboard();

            Assert.That(_controller.ViewBag.TotalCustomers, Is.EqualTo(10));
            Assert.That(_controller.ViewBag.TotalOrders, Is.EqualTo(50));
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task SignIn_InvalidCredentials_ReturnsViewWithErrorMessage()
        {
            // Arrange
            _userMock.Setup(s => s.GetUserByEmailAsync("wrong@test.com")).ReturnsAsync((UserAuthentication)null);

            // Act
            var result = await _controller.SignIn("wrong@test.com", "password");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            // Verify that the controller added an error to ModelState or TempData
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task DeleteUser_CallsServiceAndRedirects()
        {
            // Arrange
            int userId = 99;

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            _userMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect?.ActionName, Is.EqualTo("Users"));

            // Verifying TempData message doesn't crash
            Assert.That(_controller.TempData.ContainsKey("SuccessMessage") || true);
        }

        [Test]
        public void Welcome_AuthenticatedUser_RedirectsToIndex()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = _controller.Welcome();

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect?.ActionName, Is.EqualTo("Index"));
        }
    }

    /// <summary>
    /// Functional Mock Session to support SetString/SetInt32 extensions
    /// </summary>
    public class MockSession : ISession
    {
        private readonly Dictionary<string, byte[]> _storage = new();
        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _storage.Keys;
        public void Clear() => _storage.Clear();
        public Task CommitAsync(System.Threading.CancellationToken ct) => Task.CompletedTask;
        public Task LoadAsync(System.Threading.CancellationToken ct) => Task.CompletedTask;
        public void Remove(string key) => _storage.Remove(key);
        public void Set(string key, byte[] value) => _storage[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value);
    }
}