using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures; // Required for TempData
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class StoreManager1ControllerTests
    {
        private Mock<IProductService> _productMock;
        private Mock<ICategoryService> _categoryMock;
        private StoreManager1Controller _controller;

        [SetUp]
        public void Setup()
        {
            _productMock = new Mock<IProductService>();
            _categoryMock = new Mock<ICategoryService>();
            _controller = new StoreManager1Controller(_productMock.Object, _categoryMock.Object);

            // Setup HttpContext and TempData
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData; // This prevents the NullReferenceException
        }

        [Test]
        public async Task AddProduct_Post_ResolvesCategoryAndRedirects()
        {
            var product = new Product { ProductName = "Cheese", Category = "Dairy" };
            var categories = new List<Category> { new Category { CategoryId = 5, CategoryName = "Dairy" } };
            _categoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            var result = await _controller.AddProduct(product) as RedirectToActionResult;

            Assert.That(product.CategoryId, Is.EqualTo(5));
            Assert.That(_controller.TempData["SuccessMessage"], Is.Not.Null);
            Assert.That(result?.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task Create_Post_HandlesImageAndSaves()
        {
            var product = new Product { ProductName = "Juice", Category = "Drinks" };
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("fake image");
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "imageFile", "test.png");

            var result = await _controller.Create(product, file) as RedirectToActionResult;

            Assert.That(product.ImageData, Does.StartWith("data:image/png;base64,"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.Not.Null);
            Assert.That(result?.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void Delete_Post_RedirectsWithTempData()
        {
            var result = _controller.Delete(1) as RedirectToActionResult;

            _productMock.Verify(s => s.DeleteProduct(1), Times.Once);
            Assert.That(_controller.TempData["DeleteMessage"], Is.EqualTo("Product deleted successfully!"));
            Assert.That(result?.ActionName, Is.EqualTo("Index"));
        }
    }
}