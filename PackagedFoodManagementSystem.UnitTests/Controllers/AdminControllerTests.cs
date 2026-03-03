using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PackageFoodManagementSystem.Application.Controllers;
using PackageFoodManagementSystem.Services.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class AdminControllerTests
    {
        private Mock<IOrderService> _orderMock;
        private Mock<IInventoryService> _inventoryMock;
        private Mock<IProductService> _productMock;
        private Mock<IBatchService> _batchMock;
        private AdminController _controller;

        [SetUp]
        public void Setup()
        {
            _orderMock = new Mock<IOrderService>();
            _inventoryMock = new Mock<IInventoryService>();
            _productMock = new Mock<IProductService>();
            _batchMock = new Mock<IBatchService>();

            _controller = new AdminController(
                _orderMock.Object,
                _inventoryMock.Object,
                _productMock.Object,
                _batchMock.Object);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, "admin-user"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task ProcessOrder_StatusProcessing_UpdatesQuantityAndRedirects()
        {
            // Arrange
            int orderId = 10;
            var order = new Order
            {
                OrderID = orderId,
                OrderStatus = "Pending", // Fixes CS9035
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        BatchID = 1,
                        Quantity = 5,
                        // If OrderItem has required members like 'ProductName', add them here:
                        // ProductName = "Test Product" 
                    }
                }
            };

            _orderMock.Setup(s => s.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _controller.ProcessOrder(orderId, "Processing");

            // Assert
            _orderMock.Verify(s => s.UpdateOrderStatus(orderId, "Processing", "admin-user", It.IsAny<string>()), Times.Once);
            _batchMock.Verify(s => s.UpdateQuantity(1, 5), Times.Once);
            Assert.IsInstanceOf<RedirectToActionResult>(result);
        }

        [Test]
        public async Task AdminInventory_ReturnsViewWithInventoryData()
        {
            // Arrange
            _inventoryMock.Setup(s => s.GetInventoryListAsync()).ReturnsAsync(new List<Inventory>());
            _productMock.Setup(s => s.GetAllProducts()).Returns(new List<Product>());

            // Act
            var result = await _controller.AdminInventory();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.That(_controller.ViewBag.TotalProducts, Is.Not.Null);
        }
    }
}