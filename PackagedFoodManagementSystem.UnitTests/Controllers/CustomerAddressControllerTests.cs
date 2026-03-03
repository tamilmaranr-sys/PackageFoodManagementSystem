using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using PackageFoodManagementSystem.Application.Controllers;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackagedFoodManagementSystem.UnitTests.Controllers
{
    [TestFixture]
    public class CustomerAddressControllerTests
    {
        private Mock<ICustomerAddressService> _serviceMock;
        private CustomerAddressController _controller;
        private ITempDataDictionary _tempData;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<ICustomerAddressService>();
            _controller = new CustomerAddressController(_serviceMock.Object);

            // Mock TempData for the Create action
            var tempDataProvider = new Mock<ITempDataProvider>();
            _tempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider.Object);
            _controller.TempData = _tempData;
        }

        [Test]
        public async Task Index_ReturnsViewWithAddresses()
        {
            // Arrange
            var addresses = new List<CustomerAddress> { new CustomerAddress { AddressId = 1 } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(addresses);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.Model, Is.EqualTo(addresses));
        }

        [Test]
        public async Task Create_ValidModel_RedirectsWithSuccessMessage()
        {
            // Arrange
            var address = new CustomerAddress
            {
                AddressId = 1,
                StreetAddress = "123 Main St",
                City = "Chennai",
                AddressType = "Home",
                PostalCode = "600001"
            };

            // Act
            var result = await _controller.Create(address);

            // Assert
            _serviceMock.Verify(s => s.AddAsync(address), Times.Once);
            Assert.That(_controller.TempData["Message"], Is.EqualTo("Address added successfully!"));
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        [Test]
        public async Task Create_InvalidModel_ReturnsIndexViewWithData()
        {
            // Arrange
            _controller.ModelState.AddModelError("StreetAddress", "Required");
            var addresses = new List<CustomerAddress>();
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(addresses);

            // Act
            var result = await _controller.Create(new CustomerAddress());

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult.ViewName, Is.EqualTo("Index"));
            _serviceMock.Verify(s => s.AddAsync(It.IsAny<CustomerAddress>()), Times.Never);
        }

        [Test]
        public async Task Delete_ValidId_ReturnsOkResult()
        {
            // Arrange
            int addressId = 1;

            // Act
            var result = await _controller.Delete(addressId);

            // Assert
            _serviceMock.Verify(s => s.DeleteAsync(addressId), Times.Once);
            Assert.That(result, Is.InstanceOf<OkResult>());
        }
    }
}