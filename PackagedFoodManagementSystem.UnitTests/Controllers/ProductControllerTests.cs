using Moq;
using NUnit.Framework; // Ensure NUnit NuGet package is installed
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PackageFoodManagementSystem.Application.Controllers;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class ProductControllerTests
    {
        private Mock<IProductService> _productMock;
        private Mock<ICategoryService> _categoryMock;
        private ProductController _controller;

        [SetUp]
        public void Setup()
        {
            _productMock = new Mock<IProductService>();
            _categoryMock = new Mock<ICategoryService>();
            _controller = new ProductController(_productMock.Object, _categoryMock.Object);
        }

        [Test]
        public void Index_ReturnsViewWithProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Pasta", Category = "Grains" },
                new Product { ProductId = 2, ProductName = "Apple", Category = "Fruit" }
            };
            _productMock.Setup(s => s.GetMenuForCustomer()).Returns(products);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.EqualTo(products));
        }

        [Test]
        public async Task Create_Get_PopulatesCategoriesAndReturnsView()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Dairy" }
            };
            _categoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.Create() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ViewBag.Categories, Is.InstanceOf<SelectList>());
        }

        [Test]
        public async Task Create_Post_ValidProduct_RedirectsToIndex()
        {
            // Arrange
            var product = new Product
            {
                ProductName = "Milk", // Fix CS9035: Required member
                Category = "Dairy",    // Fix CS9035: Required member
                Price = 2.50m,
                CategoryId = 1
            };

            // Act
            var result = await _controller.Create(product);

            // Assert
            _productMock.Verify(s => s.CreateProduct(product), Times.Once);
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect?.ActionName, Is.EqualTo(nameof(_controller.Index)));
        }

        [Test]
        public async Task Create_Post_ResolvesCategoryIdByName_WhenNotProvided()
        {
            // Arrange
            var product = new Product
            {
                ProductName = "Cheese",
                Category = "Dairy",
                CategoryId = 0,
                Price = 5.00m
            };

            var categories = new List<Category>
            {
                new Category { CategoryId = 10, CategoryName = "Dairy" }
            };
            _categoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            // Act
            await _controller.Create(product);

            // Assert
            Assert.That(product.CategoryId, Is.EqualTo(10));
        }

        [Test]
        public async Task Create_Post_InvalidModel_ReturnsViewWithProduct()
        {
            // Arrange
            _controller.ModelState.AddModelError("ProductName", "Required");
            var product = new Product
            {
                ProductName = "",
                Category = "None"
            };

            _categoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Create(product) as ViewResult;

            // Assert
            Assert.That(result?.Model, Is.EqualTo(product));
            _productMock.Verify(s => s.CreateProduct(It.IsAny<Product>()), Times.Never);
        }
    }
}