using Moq;
using NUnit.Framework;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Implementations;
using PackageFoodManagementSystem.Services.Interfaces;
using System;

namespace PackagedFoodManagementSystem.UnitTests.Services
{
    [TestFixture]
    public class PaymentServiceTests
    {
        private Mock<IBillRepository> _billRepoMock;
        private Mock<IPaymentRepository> _paymentRepoMock;
        private Mock<IOrderService> _orderServiceMock;
        private PaymentService _service;

        [SetUp]
        public void Setup()
        {
            _billRepoMock = new Mock<IBillRepository>();
            _paymentRepoMock = new Mock<IPaymentRepository>();
            _orderServiceMock = new Mock<IOrderService>();

            _service = new PaymentService(
                _billRepoMock.Object,
                _paymentRepoMock.Object,
                _orderServiceMock.Object);
        }

        [Test]
        public void ConfirmPayment_ReturnsFalse_WhenOrderNotFound()
        {
            // Arrange
            _orderServiceMock.Setup(s => s.GetOrderById(1)).Returns((Order?)null);

            // Act
            var result = _service.ConfirmPayment(1, "Card", "1234123412341234");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Order not found"));
        }

        [Test]
        public void ConfirmPayment_ReturnsFalse_WhenBillNotFound()
        {
            // Arrange
            // FIX CS9035: Set required OrderStatus property
            _orderServiceMock.Setup(s => s.GetOrderById(1))
                .Returns(new Order { OrderID = 1, OrderStatus = "Pending" });

            _billRepoMock.Setup(r => r.GetBillByOrderId(1)).Returns((Bill?)null);

            // Act
            var result = _service.ConfirmPayment(1, "Card", "1234123412341234");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Bill not found for this order."));
        }

        [Test]
        public void ConfirmPayment_DeclinesCard_WhenNumberIsAllZeros()
        {
            // Arrange
            _orderServiceMock.Setup(s => s.GetOrderById(1))
                .Returns(new Order { OrderID = 1, OrderStatus = "Pending" });

            _billRepoMock.Setup(r => r.GetBillByOrderId(1))
                .Returns(new Bill { BillID = 10, FinalAmount = 100m });

            // Act
            var result = _service.ConfirmPayment(1, "Card", "0000 0000 0000 0000");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Card declined by bank."));

            // Verify status was updated to failed
            _orderServiceMock.Verify(s => s.UpdateOrderStatus(
                1, "Payment Failed", "System", It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ConfirmPayment_ProcessesSuccess_ForValidCard()
        {
            // Arrange
            var bill = new Bill { BillID = 10, FinalAmount = 150m, BillingStatus = "Pending" };
            _orderServiceMock.Setup(s => s.GetOrderById(1))
                .Returns(new Order { OrderID = 1, OrderStatus = "Pending" });

            _billRepoMock.Setup(r => r.GetBillByOrderId(1)).Returns(bill);

            // Act
            var result = _service.ConfirmPayment(1, "Card", "1234567812345678");

            // Assert
            Assert.That(result.Success, Is.True);
            _billRepoMock.Verify(r => r.UpdateBill(It.Is<Bill>(b => b.BillingStatus == "Paid")), Times.Once);
            _paymentRepoMock.Verify(r => r.AddPayment(It.Is<Payment>(p => p.PaymentStatus == "Success")), Times.Once);
            _orderServiceMock.Verify(s => s.UpdateOrderStatus(
                1, "Confirmed", "System_Auto_Payment", It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ConfirmPayment_SetsStatusToPlaced_ForCOD()
        {
            // Arrange
            var bill = new Bill { BillID = 20, FinalAmount = 50m };
            _orderServiceMock.Setup(s => s.GetOrderById(2))
                .Returns(new Order { OrderID = 2, OrderStatus = "Pending" });

            _billRepoMock.Setup(r => r.GetBillByOrderId(2)).Returns(bill);

            // Act
            var result = _service.ConfirmPayment(2, "COD", null);

            // Assert
            Assert.That(result.Success, Is.True);
            _paymentRepoMock.Verify(r => r.AddPayment(It.Is<Payment>(p => p.PaymentStatus == "Pending")), Times.Once);
            _orderServiceMock.Verify(s => s.UpdateOrderStatus(
                2, "Placed", "System_Auto_Payment", It.IsAny<string>()), Times.Once);
        }
    }
}