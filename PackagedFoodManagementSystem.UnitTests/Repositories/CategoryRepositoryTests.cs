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
    public class CategoryRepositoryTests
    {
        private ApplicationDbContext _context;
        private CategoryRepository _repository;

        [SetUp]
        public void Setup()
        {
            // Use a unique database name for each test to ensure isolation
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new CategoryRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllAsync_ReturnsCategories_OrderedByName()
        {
            // Arrange
            _context.Categories.AddRange(new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Dairy" },
                new Category { CategoryId = 2, CategoryName = "Bakery" },
                new Category { CategoryId = 3, CategoryName = "Beverages" }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();
            var list = result.ToList();

            // Assert
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0].CategoryName, Is.EqualTo("Bakery")); // Alphabetical order
            Assert.That(list[1].CategoryName, Is.EqualTo("Beverages"));
            Assert.That(list[2].CategoryName, Is.EqualTo("Dairy"));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsCorrectCategory_WhenIdExists()
        {
            // Arrange
            var category = new Category { CategoryId = 10, CategoryName = "Snacks" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CategoryName, Is.EqualTo("Snacks"));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}