using PackageFoodManagementSystem.Services.Helpers;
using PackageFoodManagementSystem.Repository.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PackageFoodManagementSystem.Services.Interfaces;
using PackageFoodManagementSystem.Repository.Interfaces;
using System.Linq;
using System;

namespace PackageFoodManagementSystem.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;

        public UserService(IUserRepository userRepository, ICustomerRepository customerRepository)
        {
            _userRepository = userRepository;
            _customerRepository = customerRepository;
        }

        public async Task<int> CreateUserAsync(string name, string mobileNumber, string email, string password, string role, CancellationToken cancellationToken = default)
        {
            var user = new UserAuthentication
            {
                Name = name,
                Email = email,
                MobileNumber = mobileNumber,
                Password = PasswordHelper.HashPassword(password),
                Role = role // <--- Change "User" to the variable 'role'
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var customer = new Customer
            {
                Name = name,
                Email = email,
                Phone = mobileNumber,
                Status = "Active",
                UserId = user.Id
            };

            await _customerRepository.AddAsync(customer);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return user.Id;
        }

        public async Task DeleteUserAsync(int id, CancellationToken cancellationToken = default)
        {
            // 1. Delete linked customer first (Foreign Key)
            var allCustomers = await _customerRepository.GetAllAsync();
            var customer = allCustomers.FirstOrDefault(c => c.UserId == id);
            if (customer != null)
            {
                await _customerRepository.DeleteAsync(customer.CustomerId);
            }

            // 2. Delete User
            await _userRepository.DeleteAsync(id, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        public Task<UserAuthentication?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
            => _userRepository.GetByEmailAsync(email, cancellationToken);

        public Task<UserAuthentication?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
            => _userRepository.GetByIdAsync(id, cancellationToken);

        public Task<List<UserAuthentication>> GetAllUsersAsync(CancellationToken cancellationToken = default)
            => _userRepository.GetAllAsync(cancellationToken);

        public async Task AddUserAsync(UserAuthentication user, CancellationToken cancellationToken = default)
        {
            user.Password = PasswordHelper.HashPassword(user.Password);
            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        public Task UpdateUserAsync(UserAuthentication user, CancellationToken cancellationToken = default)
            => _userRepository.UpdateAsync(user, cancellationToken);

        public Task<int> CountUsersByRoleAsync(string role, CancellationToken cancellationToken = default)
            => _userRepository.CountByRoleAsync(role, cancellationToken);

        // Fixing the previously throwing methods:
        public async Task<UserAuthentication?> AuthenticateAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user != null && PasswordHelper.VerifyPassword(password, user.Password)) return user;
            return null;
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterUserAsync(UserAuthentication user)
        {
            try
            {
                await CreateUserAsync(user.Name, user.MobileNumber, user.Email, user.Password, user.Role);
                return (true, null);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(UserAuthentication user)

        {

            var existingUser = await _userRepository.GetUserByIdAsync(user.Id);

            if (existingUser == null) return (false, "User not found");

            existingUser.Name = user.Name;

            existingUser.Email = user.Email;

            existingUser.MobileNumber = user.MobileNumber;

            existingUser.Role = user.Role;

            if (!string.IsNullOrEmpty(user.Password))

                existingUser.Password = PasswordHelper.HashPassword(user.Password);

            await _userRepository.SaveChangesAsync();

            return (true, null);

        }


        public Task<(int TotalCustomers, int TotalStoreManagers, int TotalOrders)> GetAdminDashboardStatsAsync()
            => _userRepository.GetDashboardStatsAsync();
    }
}