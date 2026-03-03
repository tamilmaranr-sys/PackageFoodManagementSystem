using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Application.Controllers;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class MenuControllerTests
    {
        private Mock<IProductService> _productMock;
        private Mock<ICartService> _cartMock;
        private MenuController _controller;

        [SetUp]
        public void Setup()
        {
            _productMock = new Mock<IProductService>();
            _cartMock = new Mock<ICartService>();
            _controller = new MenuController(_productMock.Object, _cartMock.Object);
        }

        [Test]
        public void Index_NoFilters_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Milk", Category = "Dairy" },
                new Product { ProductId = 2, ProductName = "Bread", Category = "Bakery" }
            };
            _productMock.Setup(s => s.GetAllProducts()).Returns(products);

            // Act
            var result = _controller.Index(null, null) as ViewResult;
            var model = result?.Model as List<Product>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(model?.Count, Is.EqualTo(2));
        }

        [Test]
        public void Index_WithSearchTerm_FiltersProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Apple Juice", Category = "Drinks" },
                new Product { ProductId = 2, ProductName = "Orange", Category = "Fruit" }
            };
            _productMock.Setup(s => s.GetAllProducts()).Returns(products);

            // Act
            var result = _controller.Index(null, "Apple") as ViewResult;
            var model = result?.Model as List<Product>;

            // Assert
            Assert.That(model?.Count, Is.EqualTo(1));
            Assert.That(model?.First().ProductName, Is.EqualTo("Apple Juice"));
            Assert.That(_controller.ViewBag.CurrentSearch, Is.EqualTo("Apple"));
        }

        [Test]
        public void Index_WithCategory_FiltersProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Milk", Category = "Dairy" },
                new Product { ProductId = 2, ProductName = "Cheese", Category = "Dairy" },
                new Product { ProductId = 3, ProductName = "Cake", Category = "Bakery" }
            };
            _productMock.Setup(s => s.GetAllProducts()).Returns(products);

            // Act
            var result = _controller.Index("Dairy", null) as ViewResult;
            var model = result?.Model as List<Product>;

            // Assert
            Assert.That(model?.Count, Is.EqualTo(2));
            Assert.That(model?.All(p => p.Category == "Dairy"), Is.True);
        }

        [Test]
        public void Details_ProductExists_ReturnsViewWithProduct()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 10,
                ProductName = "Chocolate",
                Category = "Sweets" // Fixed CS9035
            };
            _productMock.Setup(s => s.GetProductById(10)).Returns(product);

            // Act
            var result = _controller.Details(10) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Model, Is.EqualTo(product));
        }

        [Test]
        public void Details_ProductDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _productMock.Setup(s => s.GetProductById(It.IsAny<int>())).Returns((Product)null!);

            // Act
            var result = _controller.Details(999);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}