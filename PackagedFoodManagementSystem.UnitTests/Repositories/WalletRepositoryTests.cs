using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PackageFoodManagementSystem.Repository.Data;
using PackageFoodManagementSystem.Repository.Implementations;
using PackageFoodManagementSystem.Repository.Models;
using System;
using System.Linq;

namespace PackagedFoodManagementSystem.UnitTests.Repositories
{
    [TestFixture]
    public class WalletRepositoryTests
    {
        private ApplicationDbContext _context;
        private WalletRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new WalletRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetByUserId_ReturnsWallet_WhenUserExists()
        {
            // Arrange
            var wallet = new Wallet { WalletId = 1, UserId = 101, Balance = 50.0m };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();

            // Act
            var result = _repository.GetByUserId(101);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Balance, Is.EqualTo(50.0m));
        }

        [Test]
        public void Add_SavesNewWallet()
        {
            // Arrange
            var wallet = new Wallet { WalletId = 2, UserId = 102, Balance = 0m };

            // Act
            _repository.Add(wallet);
            _repository.Save();

            // Assert
            var savedWallet = _context.Wallets.FirstOrDefault(w => w.UserId == 102);
            Assert.That(savedWallet, Is.Not.Null);
            Assert.That(savedWallet.Balance, Is.EqualTo(0m));
        }

        [Test]
        public void Update_ModifiesBalance_Successfully()
        {
            // Arrange
            var wallet = new Wallet { WalletId = 3, UserId = 103, Balance = 10.0m };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();

            // Act
            wallet.Balance += 20.0m;
            _repository.Update(wallet);
            _repository.Save();

            // Assert
            var updatedWallet = _context.Wallets.Find(3);
            Assert.That(updatedWallet.Balance, Is.EqualTo(30.0m));
        }

        [Test]
        public void GetByUserId_ReturnsNull_WhenUserDoesNotExist()
        {
            // Act
            var result = _repository.GetByUserId(999);

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}