using Moq;
using NUnit.Framework;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Implementations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackagedFoodManagementSystem.UnitTests.Services
{
    [TestFixture]
    public class InventoryServiceTests
    {
        private Mock<IInventoryRepository> _repoMock;
        private InventoryService _service;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IInventoryRepository>();
            _service = new InventoryService(_repoMock.Object);
        }

        [Test]
        public async Task GetInventoryListAsync_ReturnsAllRecords()
        {
            // Arrange
            var list = new List<Inventory>
            {
                new Inventory { Id = 1, ProductId = 101, StockQuantity = 10, Category = "Dairy" },
                new Inventory { Id = 2, ProductId = 102, StockQuantity = 20, Category = "Bakery" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            // Act
            var result = await _service.GetInventoryListAsync();

            // Assert
            Assert.That(result, Is.EqualTo(list));
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task AddBatchToInventoryAsync_CreatesEntryWithProductCategory()
        {
            // Arrange
            var batch = new Batch { BatchId = 1, ProductId = 50, Quantity = 100 };

            // FIX CS9035: Initialize required members (ProductName, Category, etc.)
            var product = new Product
            {
                ProductId = 50,
                ProductName = "Test Product",
                Category = "Frozen Foods",
                Price = 10.0m
            };

            _repoMock.Setup(r => r.GetProductByIdAsync(50)).ReturnsAsync(product);

            // Act
            await _service.AddBatchToInventoryAsync(batch);

            // Assert
            _repoMock.Verify(r => r.AddBatchAsync(
                batch,
                It.Is<Inventory>(i => i.ProductId == 50 && i.Category == "Frozen Foods" && i.StockQuantity == 100)
            ), Times.Once);
        }

        [Test]
        public async Task AddBatchToInventoryAsync_UsesDefaultCategory_WhenProductNotFound()
        {
            // Arrange
            var batch = new Batch { ProductId = 99, Quantity = 5 };
            _repoMock.Setup(r => r.GetProductByIdAsync(99)).ReturnsAsync((Product?)null);

            // Act
            await _service.AddBatchToInventoryAsync(batch);

            // Assert
            _repoMock.Verify(r => r.AddBatchAsync(
                batch,
                It.Is<Inventory>(i => i.Category == "General")
            ), Times.Once);
        }
    }
}