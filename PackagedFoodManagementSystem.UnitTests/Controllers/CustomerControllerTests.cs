using NUnit.Framework;
using PackageFoodManagementSystem.Repository.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PackagedFoodManagementSystem.UnitTests.Models
{
    [TestFixture]
    public class CustomerModelTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Test]
        public void Customer_ValidData_PassesValidation()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerId = 1,
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                Status = "Active"
            };

            // Act
            var results = ValidateModel(customer);

            // Assert
            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Customer_MissingRequiredFields_FailsValidation()
        {
            // Arrange - Name, Email, and Phone are Required
            var customer = new Customer();

            // Act
            var results = ValidateModel(customer);

            // Assert
            Assert.That(results.Any(r => r.MemberNames.Contains("Name")), Is.True);
            Assert.That(results.Any(r => r.MemberNames.Contains("Email")), Is.True);
            Assert.That(results.Any(r => r.MemberNames.Contains("Phone")), Is.True);
        }

        [Test]
        public void Customer_InvalidEmail_FailsValidation()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test",
                Email = "not-an-email",
                Phone = "1234567890"
            };

            // Act
            var results = ValidateModel(customer);

            // Assert
            Assert.That(results.Any(r => r.MemberNames.Contains("Email")), Is.True);
        }

        [Test]
        public void Customer_NavigationProperties_CanBeAssigned()
        {
            // Arrange
            var customer = new Customer();
            var addresses = new List<CustomerAddress> { new CustomerAddress { AddressId = 1 } };
            var userAuth = new UserAuthentication { Id = 10 };

            // Act
            customer.Addresses = addresses;
            customer.User = userAuth;

            // Assert
            Assert.That(customer.Addresses.Count, Is.EqualTo(1));
            Assert.That(customer.User.Id, Is.EqualTo(10));
        }
    }
}