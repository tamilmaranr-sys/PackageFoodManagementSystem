using Microsoft.EntityFrameworkCore;
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
    public class BatchRepositoryTests
    {
        private ApplicationDbContext _context;
        private BatchRepository _repository;

        [SetUp]
        public void Setup()
        {
            // Create a unique database name for every test to ensure isolation
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new BatchRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddBatchAsync_AddsBatchToDatabase()
        {
            // Arrange
            var batch = new Batch { BatchId = 1, ProductId = 10, Quantity = 100, ExpiryDate = DateTime.Now.AddMonths(6) };

            // Act
            await _repository.AddBatchAsync(batch);

            // Assert
            var result = await _context.Batches.FindAsync(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductId, Is.EqualTo(10));
        }

        [Test]
        public async Task GetAllBatchesAsync_ReturnsAllBatches()
        {
            // Arrange
            _context.Batches.AddRange(new List<Batch>
            {
                new Batch { BatchId = 1, ProductId = 10, Quantity = 50, ExpiryDate = DateTime.Now },
                new Batch { BatchId = 2, ProductId = 11, Quantity = 30, ExpiryDate = DateTime.Now }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllBatchesAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetBatchesByProductIdAsync_ReturnsFilteredResults()
        {
            // Arrange
            int targetProductId = 10;
            _context.Batches.AddRange(new List<Batch>
            {
                new Batch { BatchId = 1, ProductId = targetProductId, Quantity = 50, ExpiryDate = DateTime.Now },
                new Batch { BatchId = 2, ProductId = 99, Quantity = 30, ExpiryDate = DateTime.Now }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBatchesByProductIdAsync(targetProductId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().ProductId, Is.EqualTo(targetProductId));
        }

        [Test]
        public async Task DeleteBatchAsync_RemovesBatch_IfItExists()
        {
            // Arrange
            var batch = new Batch { BatchId = 5, ProductId = 10, Quantity = 10, ExpiryDate = DateTime.Now };
            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteBatchAsync(5);

            // Assert
            var result = await _context.Batches.FindAsync(5);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateBatchAsync_ModifiesExistingData()
        {
            // Arrange
            var batch = new Batch { BatchId = 1, ProductId = 10, Quantity = 50, ExpiryDate = DateTime.Now };
            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();

            // Act
            batch.Quantity = 500;
            await _repository.UpdateBatchAsync(batch);

            // Assert
            var updated = await _context.Batches.FindAsync(1);
            Assert.That(updated.Quantity, Is.EqualTo(500));
        }
    }
}