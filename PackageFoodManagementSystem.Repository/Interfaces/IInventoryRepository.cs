using PackageFoodManagementSystem.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PackageFoodManagementSystem.Repository.Interfaces
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task AddBatchAsync(Batch batch, Inventory inventory);
        Task<Product> GetProductByIdAsync(int productId);
    }
}
