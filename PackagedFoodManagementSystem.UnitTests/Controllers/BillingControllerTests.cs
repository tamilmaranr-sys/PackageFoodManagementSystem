using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Application.Controllers;
using PackageFoodManagementSystem.Services.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using System;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class BillingControllerTests
    {
        private Mock<IBillingService> _billingMock;
        private BillingController _controller;

        [SetUp]
        public void Setup()
        {
            _billingMock = new Mock<IBillingService>();
            _controller = new BillingController(_billingMock.Object);
        }

        [Test]
        public void Generate_ValidOrderId_CallsServiceAndRedirects()
        {
            // Arrange
            int testOrderId = 501;

            // Act
            var result = _controller.Generate(testOrderId);

            // Assert
            // Verify the service was called with the correct ID
            _billingMock.Verify(s => s.GenerateBill(testOrderId), Times.Once);

            // Verify redirection to Order Index
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Order"));
        }

        [Test]
        public void Generate_NegativeOrderId_StillCallsService()
        {
            // Arrange
            int invalidId = -1;

            // Act
            var result = _controller.Generate(invalidId);

            // Assert
            _billingMock.Verify(s => s.GenerateBill(invalidId), Times.Once);
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }
    }
}