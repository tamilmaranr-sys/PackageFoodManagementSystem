using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using PackageFoodManagementSystem.Repository.Data;
using PackageFoodManagementSystem.Repository.Implementations;
using PackageFoodManagementSystem.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackagedFoodManagementSystem.UnitTests.Repositories
{
    [TestFixture]
    public class InventoryRepositoryTests
    {
        private ApplicationDbContext _context;
        private InventoryRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                // FIX: Tell the In-Memory provider to ignore transaction warnings
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new InventoryRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllAsync_ReturnsInventory_WithIncludes()
        {
            // Arrange
            var product = new Product { ProductId = 1, ProductName = "Rice", Category = "Grains", Price = 10.0m };
            var batch = new Batch { BatchId = 1, ProductId = 1, Quantity = 100, ExpiryDate = DateTime.Now.AddMonths(6) };
            // Ensure property name matches your model (Id vs InventoryId)
            var inventory = new Inventory { Id = 1, ProductId = 1, BatchId = 1, StockQuantity = 100, Category = "Grains" };

            _context.Products.Add(product);
            _context.Batches.Add(batch);
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();
            var item = result.FirstOrDefault();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(item.Product, Is.Not.Null);
            Assert.That(item.Batch, Is.Not.Null);
        }

        [Test]
        public async Task GetProductByIdAsync_ReturnsCorrectProduct()
        {
            // Arrange
            var product = new Product { ProductId = 10, ProductName = "Bread", Category = "Bakery", Price = 2.5m };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProductByIdAsync(10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductName, Is.EqualTo("Bread"));
        }

        [Test]
        public async Task AddBatchAsync_UpdatesProductQuantity_AndSavesInventory()
        {
            // Arrange
            var product = new Product { ProductId = 5, ProductName = "Milk", Category = "Dairy", Price = 3.0m, Quantity = 10 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var batch = new Batch { BatchId = 2, ProductId = 5, Quantity = 50 };
            var inventory = new Inventory { ProductId = 5, StockQuantity = 50, Category = "Dairy" };

            // Act
            await _repository.AddBatchAsync(batch, inventory);

            // Assert
            var updatedProduct = await _context.Products.FindAsync(5);
            var savedInventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == 5);

            Assert.That(updatedProduct.Quantity, Is.EqualTo(60));
            Assert.That(savedInventory, Is.Not.Null);
            Assert.That(savedInventory.BatchId, Is.EqualTo(2));
        }
    }
}