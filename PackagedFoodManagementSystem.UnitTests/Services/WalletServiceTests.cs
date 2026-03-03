using Moq;
using NUnit.Framework;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Implementations;

namespace PackagedFoodManagementSystem.UnitTests.Services
{
    [TestFixture]
    public class WalletServiceTests
    {
        private Mock<IWalletRepository> _repoMock;
        private WalletService _service;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IWalletRepository>();
            _service = new WalletService(_repoMock.Object);
        }

        [Test]
        public void CreateIfMissing_ReturnsExistingWallet_WhenFound()
        {
            // Arrange
            var existingWallet = new Wallet { UserId = 1, Balance = 100m };
            _repoMock.Setup(r => r.GetByUserId(1)).Returns(existingWallet);

            // Act
            var result = _service.CreateIfMissing(1);

            // Assert
            Assert.That(result, Is.EqualTo(existingWallet));
            _repoMock.Verify(r => r.Add(It.IsAny<Wallet>()), Times.Never);
        }

        [Test]
        public void CreateIfMissing_CreatesAndReturnsNewWallet_WhenNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByUserId(1)).Returns((Wallet?)null);

            // Act
            var result = _service.CreateIfMissing(1);

            // Assert
            Assert.That(result.UserId, Is.EqualTo(1));
            Assert.That(result.Balance, Is.EqualTo(0m));
            _repoMock.Verify(r => r.Add(It.IsAny<Wallet>()), Times.Once);
            _repoMock.Verify(r => r.Save(), Times.Once);
        }

        [Test]
        public void AddMoney_IncrementsBalance_AndSaves()
        {
            // Arrange
            var wallet = new Wallet { UserId = 1, Balance = 50m };
            _repoMock.Setup(r => r.GetByUserId(1)).Returns(wallet);

            // Act
            _service.AddMoney(1, 25m);

            // Assert
            Assert.That(wallet.Balance, Is.EqualTo(75m));
            _repoMock.Verify(r => r.Update(wallet), Times.Once);
            _repoMock.Verify(r => r.Save(), Times.AtLeastOnce);
        }

        [Test]
        public void AddMoney_CreatesWalletFirst_IfMissing()
        {
            // Arrange
            _repoMock.SetupSequence(r => r.GetByUserId(1))
                .Returns((Wallet?)null) // First call in AddMoney
                .Returns((Wallet?)null) // Second call inside CreateIfMissing
                .Returns(new Wallet { UserId = 1, Balance = 0m }); // Final retrieval

            // Act
            _service.AddMoney(1, 100m);

            // Assert
            _repoMock.Verify(r => r.Add(It.Is<Wallet>(w => w.UserId == 1)), Times.Once);
        }
    }
}