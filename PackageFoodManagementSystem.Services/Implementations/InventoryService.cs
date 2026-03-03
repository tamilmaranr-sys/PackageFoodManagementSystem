
using Microsoft.EntityFrameworkCore;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Text;

namespace PackageFoodManagementSystem.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepo;

        public InventoryService(IInventoryRepository inventoryRepo)
        {
            _inventoryRepo = inventoryRepo;
        }

        public async Task<IEnumerable<Inventory>> GetInventoryListAsync()
        {
            // This simply asks the Repository for the joined data we defined above
            return await _inventoryRepo.GetAllAsync();

        }

        public async Task AddBatchToInventoryAsync(Batch batch)
        {

            // 1. Get the product to find the correct category name

            var product = await _inventoryRepo.GetProductByIdAsync(batch.ProductId);            // Prepare the Inventory record mapping
            var inventoryEntry = new Inventory
            {
                ProductId = batch.ProductId,
                StockQuantity = batch.Quantity,
                // You can fetch the category from the product if needed here
                Category = product?.Category ?? "General",
            };

            // Call the repository to handle the DB transaction
            await _inventoryRepo.AddBatchAsync(batch, inventoryEntry);
        }


    }
}