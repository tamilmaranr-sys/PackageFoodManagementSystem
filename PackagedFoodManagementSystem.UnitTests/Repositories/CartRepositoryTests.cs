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
    public class CartRepositoryTests
    {
        private ApplicationDbContext _context;
        private CartRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new CartRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetActiveCartByUserIdAsync_ReturnsActiveCart_WithIncludedItems()
        {
            var product = new Product { ProductId = 1, ProductName = "Milk", Category = "Dairy", Price = 2.5m };
            var cart = new Cart
            {
                CartId = 1,
                UserAuthenticationId = 101,
                IsActive = true,
                CartItems = new List<CartItem> { new CartItem { ProductId = 1, Quantity = 2, Product = product } }
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var result = await _repository.GetActiveCartByUserIdAsync(101);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.CartItems.ElementAt(0).Product.ProductName, Is.EqualTo("Milk"));
        }

        [Test]
        public async Task AddAsync_AddsNewCartToDatabase()
        {
            var cart = new Cart { CartId = 2, UserAuthenticationId = 102, IsActive = true };
            await _repository.AddAsync(cart);
            await _repository.SaveChangesAsync();

            var result = await _context.Carts.FindAsync(2);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task AddItemAsync_AddsItemToCartItemsTable()
        {
            var item = new CartItem { CartId = 1, ProductId = 1, Quantity = 5 };
            await _repository.AddItemAsync(item);
            await _repository.SaveChangesAsync();

            var result = await _context.CartItems.FirstOrDefaultAsync(x => x.CartId == 1);
            Assert.That(result, Is.Not.Null);
        }


        [Test]
        public async Task RemoveItemAsync_RemovesSpecifiedItem()
        {
            // Arrange
            var item = new CartItem { CartId = 10, ProductId = 20, Quantity = 1 };
            _context.CartItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            await _repository.RemoveItemAsync(item);
            await _repository.SaveChangesAsync();

            // Assert 
            // FIX: Use FirstOrDefaultAsync with a predicate instead of Find() 
            // to avoid composite key errors.
            var result = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == 10 && ci.ProductId == 20);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetActiveCartByUserIdAsync_ReturnsNull_WhenCartIsInactive()
        {
            var cart = new Cart { CartId = 3, UserAuthenticationId = 103, IsActive = false };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var result = await _repository.GetActiveCartByUserIdAsync(103);
            Assert.That(result, Is.Null);
        }
    }
}