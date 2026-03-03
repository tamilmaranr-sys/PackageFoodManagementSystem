using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PackageFoodManagementSystem.Services.Implementations
{
    public class CustomerService : ICustomerService // Add the interface here
    {
        private readonly ICustomerRepository _repository;
        public CustomerService(ICustomerRepository repository) => _repository = repository;

        public Task<IEnumerable<Customer>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Customer> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task AddAsync(Customer customer) => _repository.AddAsync(customer);
        public Task UpdateAsync(Customer customer) => _repository.UpdateAsync(customer);
        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            // If your repo doesn’t have this method yet, filter in memory
            var all = await _repository.GetAllAsync();
            return all.FirstOrDefault(c => c.UserId == userId);
        }
    }
}
