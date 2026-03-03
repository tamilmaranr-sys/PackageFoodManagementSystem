using PackageFoodManagementSystem.Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Services.Interfaces
{
    public interface IBatchService
    {
        Task<IEnumerable<Batch>> GetAllBatchesAsync();
        Task<Batch?> GetBatchByIdAsync(int id);
        Task AddBatchAsync(Batch batch);
        Task UpdateBatchAsync(Batch batch);
        Task DeleteBatchAsync(int id);
        Task UpdateQuantity(int batchId, int quantityChange);
        Task SyncProductTotalQuantityAsync(int productId);
        Task<IEnumerable<Batch>> GetBatchesByProductIdAsync(int productId);

        // Optional FIFO method (if you plan to call from OrderService)
        Task ReduceStockFifoAsync(int productId, int quantityToReduce);
    }
}