using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Controllers;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class BatchControllerTests
    {
        private Mock<IBatchService> _batchMock;
        private Mock<IProductService> _productMock;
        private Mock<IInventoryService> _inventoryMock;
        private Mock<ICategoryService> _categoryMock;
        private BatchController _controller;

        [SetUp]
        public void Setup()
        {
            _batchMock = new Mock<IBatchService>();
            _productMock = new Mock<IProductService>();
            _inventoryMock = new Mock<IInventoryService>();
            _categoryMock = new Mock<ICategoryService>();

            _controller = new BatchController(
                _batchMock.Object,
                _productMock.Object,
                _inventoryMock.Object,
                _categoryMock.Object);
        }

        [Test]
        public async Task Index_ReturnsViewWithBatches()
        {
            // Arrange
            var batches = new List<Batch> { new Batch { BatchId = 1, Quantity = 100 } };
            _batchMock.Setup(s => s.GetAllBatchesAsync()).ReturnsAsync(batches);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.Model, Is.EqualTo(batches));
        }

        [Test]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var batch = new Batch
            {
                BatchId = 1,
                ProductId = 101,
                Quantity = 50,
                ManufactureDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddMonths(6)
            };

            // Act
            var result = await _controller.Create(batch);

            // Assert
            _batchMock.Verify(s => s.AddBatchAsync(batch), Times.Once);
            _inventoryMock.Verify(s => s.AddBatchToInventoryAsync(batch), Times.Once);
            _batchMock.Verify(s => s.SyncProductTotalQuantityAsync(101), Times.Once);
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        [Test]
        public async Task Edit_Get_ValidId_ReturnsView()
        {
            // Arrange
            int batchId = 1;
            var batch = new Batch { BatchId = batchId, ProductId = 5, CategoryId = 2 };

            _batchMock.Setup(s => s.GetBatchByIdAsync(batchId)).ReturnsAsync(batch);
            _categoryMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Category>());
            _productMock.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<Product>());

            // Act
            var result = await _controller.Edit(batchId);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Delete_Post_RedirectsToIndex()
        {
            // Arrange
            int batchId = 1;

            // Act
            var result = await _controller.Delete(batchId);

            // Assert
            _batchMock.Verify(s => s.DeleteBatchAsync(batchId), Times.Once);
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }
    }
}