using Moq;
using NUnit.Framework;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackagedFoodManagementSystem.UnitTests.Services
{
    [TestFixture]
    public class BatchServiceTests
    {
        private Mock<IBatchRepository> _batchRepoMock;
        private Mock<IProductRepository> _productRepoMock;
        private BatchService _service;

        [SetUp]
        public void Setup()
        {
            _batchRepoMock = new Mock<IBatchRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _service = new BatchService(_batchRepoMock.Object, _productRepoMock.Object);
        }

        [Test]
        public async Task AddBatchAsync_CallsRepository_AndSyncsQuantity()
        {
            // Arrange
            var batch = new Batch { BatchId = 1, ProductId = 10, Quantity = 50 };
            var product = new Product { ProductId = 10, ProductName = "Milk", Category = "Dairy", Quantity = 0 };

            _batchRepoMock.Setup(r => r.GetBatchesByProductIdAsync(10))
                .ReturnsAsync(new List<Batch> { batch });
            _productRepoMock.Setup(r => r.GetProductById(10)).Returns(product);

            // Act
            await _service.AddBatchAsync(batch);

            // Assert
            _batchRepoMock.Verify(r => r.AddBatchAsync(batch), Times.Once);
            _productRepoMock.Verify(r => r.UpdateProduct(It.Is<Product>(p => p.Quantity == 50)), Times.Once);
        }

        [Test]
        public async Task ReduceStockFifoAsync_ReducesOldestBatchesFirst()
        {
            // Arrange
            int productId = 10;
            var batch1 = new Batch { BatchId = 1, ProductId = productId, Quantity = 10, ExpiryDate = DateTime.Now.AddDays(1) };
            var batch2 = new Batch { BatchId = 2, ProductId = productId, Quantity = 20, ExpiryDate = DateTime.Now.AddDays(5) };

            _batchRepoMock.Setup(r => r.GetBatchesByProductIdAsync(productId))
                .ReturnsAsync(new List<Batch> { batch2, batch1 }); // Out of order

            _productRepoMock.Setup(r => r.GetProductById(productId))
                .Returns(new Product { ProductId = productId, ProductName = "Test", Category = "Cat" });

            // Act - Reduce 15 units
            await _service.ReduceStockFifoAsync(productId, 15);

            // Assert
            Assert.That(batch1.Quantity, Is.EqualTo(0)); // Oldest fully depleted
            Assert.That(batch2.Quantity, Is.EqualTo(15)); // 5 taken from second oldest
            _batchRepoMock.Verify(r => r.UpdateBatchAsync(It.IsAny<Batch>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task UpdateQuantity_DecreasesQuantity_AndSyncs()
        {
            // Arrange
            var batch = new Batch { BatchId = 1, ProductId = 5, Quantity = 10 };
            _batchRepoMock.Setup(r => r.GetBatchByIdAsync(1)).ReturnsAsync(batch);
            _batchRepoMock.Setup(r => r.GetBatchesByProductIdAsync(5)).ReturnsAsync(new List<Batch> { batch });
            _productRepoMock.Setup(r => r.GetProductById(5)).Returns(new Product { ProductId = 5, ProductName = "X", Category = "Y" });

            // Act
            await _service.UpdateQuantity(1, 4);

            // Assert
            Assert.That(batch.Quantity, Is.EqualTo(6));
            _batchRepoMock.Verify(r => r.UpdateBatchAsync(batch), Times.Once);
        }

        [Test]
        public async Task DeleteBatchAsync_RemovesBatch_AndUpdatesProductTotal()
        {
            // Arrange
            var batch = new Batch { BatchId = 1, ProductId = 10, Quantity = 50 };
            _batchRepoMock.Setup(r => r.GetBatchByIdAsync(1)).ReturnsAsync(batch);
            _batchRepoMock.Setup(r => r.GetBatchesByProductIdAsync(10)).ReturnsAsync(new List<Batch>()); // Empty after delete
            _productRepoMock.Setup(r => r.GetProductById(10)).Returns(new Product { ProductId = 10, ProductName = "X", Category = "Y" });

            // Act
            await _service.DeleteBatchAsync(1);

            // Assert
            _batchRepoMock.Verify(r => r.DeleteBatchAsync(1), Times.Once);
            _productRepoMock.Verify(r => r.UpdateProduct(It.Is<Product>(p => p.Quantity == 0)), Times.Once);
        }
    }
}