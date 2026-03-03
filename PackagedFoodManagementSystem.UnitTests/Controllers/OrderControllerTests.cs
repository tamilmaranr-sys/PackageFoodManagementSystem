using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Application.Controllers;
using PackageFoodManagementSystem.Services.Interfaces;
using PackageFoodManagementSystem.Repository.Data;
using PackageFoodManagementSystem.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class OrderControllerTests
    {
        private ApplicationDbContext _context;
        private Mock<IOrderService> _orderService;
        private OrderController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("OrderCtrlDb")
                .Options;
            _context = new ApplicationDbContext(options);
            _orderService = new Mock<IOrderService>();
            _controller = new OrderController(_context, _orderService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void Index_ReturnsView()
        {
            // Arrange
            _orderService.Setup(o => o.GetAllOrders()).Returns(new List<Order>());

            // Act
            var res = _controller.Index();

            // Assert
            Assert.IsInstanceOf<ViewResult>(res);
        }

        [Test]
        public void Create_Post_Redirects()
        {
            // Arrange - OrderStatus is 'required' in your Model
            var o = new Order
            {
                CustomerId = 1,
                CreatedByUserID = 1,
                OrderStatus = "Pending", // Set required member
                TotalAmount = 0,
                OrderDate = DateTime.Now
            };

            // Act
            var res = _controller.Create(o);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(res);
            _orderService.Verify(x => x.PlaceOrder(o), Times.Once);
        }

        [Test]
        public void Checkout_RedirectsToLogin_WhenNotAuthenticated()
        {
            // Arrange: No claims = Not Authenticated
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var httpContext = new DefaultHttpContext { User = user };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var res = _controller.Checkout();

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(res);
        }

        [Test]
        public void PlaceOrder_Post_RedirectsToPayment()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "test"));

            var http = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = http };

            _orderService.Setup(o => o.CreateOrder(1, "addr")).Returns(5);

            // Act
            var res = _controller.PlaceOrder("addr");

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(res);
        }
    }
}