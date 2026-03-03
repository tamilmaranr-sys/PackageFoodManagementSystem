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
    public class CategoryServiceTests
    {
        private Mock<ICategoryRepository> _repoMock;
        private CategoryService _service;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<ICategoryRepository>();
            _service = new CategoryService(_repoMock.Object);
        }

        [Test]
        public async Task GetAllAsync_CallsRepository_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Fruits" },
                new Category { CategoryId = 2, CategoryName = "Vegetables" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.That(result, Is.EqualTo(categories));
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Test]
        public async Task GetByIdAsync_CallsRepository_ReturnsCorrectCategory()
        {
            // Arrange
            var category = new Category { CategoryId = 10, CategoryName = "Snacks" };
            _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(category);

            // Act
            var result = await _service.GetByIdAsync(10);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CategoryId, Is.EqualTo(10));
            Assert.That(result.CategoryName, Is.EqualTo("Snacks"));
            _repoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
        }

        [Test]
        public async Task GetByIdAsync_ReturnsNull_WhenCategoryNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
            _repoMock.Verify(r => r.GetByIdAsync(999), Times.Once);
        }
    }
}