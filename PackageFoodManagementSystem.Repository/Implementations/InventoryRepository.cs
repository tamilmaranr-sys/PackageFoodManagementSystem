
using Microsoft.EntityFrameworkCore;
using PackageFoodManagementSystem.Repository.Data;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Repository.Interfaces;

namespace PackageFoodManagementSystem.Repository.Implementations
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;
        public InventoryRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _context.Inventories
                .Include(i => i.Product)      // To get Product Name
                .Include(i => i.Batch)        // To get Batch Details from Store Manager
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            // Use the context injected into your Repository
            return await _context.Products.FindAsync(productId);
        }

        public async Task AddBatchAsync(Batch batch, Inventory inventory)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Add the Batch


                // 2. Assign the newly generated BatchId to Inventory and add it
                inventory.BatchId = batch.BatchId;
                _context.Inventories.Add(inventory);

                // 3. Update Product Total Quantity
                var product = await _context.Products.FindAsync(batch.ProductId);
                if (product != null)
                {
                    product.Quantity += batch.Quantity;
                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


    }
}