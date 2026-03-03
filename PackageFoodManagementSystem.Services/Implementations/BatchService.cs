using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Services.Implementations
{
    public class BatchService : IBatchService
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IProductRepository _productRepository;

        public BatchService(IBatchRepository batchRepository, IProductRepository productRepository)
        {
            _batchRepository = batchRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Batch>> GetAllBatchesAsync()
            => await _batchRepository.GetAllBatchesAsync();

        public async Task AddBatchAsync(Batch batch)
        {
            // --- FIX: Fetch the Product to get its CategoryId ---
            var product = _productRepository.GetProductById(batch.ProductId);

            if (product != null)
            {
                // Automatically assign the CategoryId from the Product to the Batch
                batch.CategoryId = product.CategoryId;
            }
            else
            {
                // Fallback: If product isn't found, you might want to throw an error 
                // or set a default ID so SQL doesn't reject it.
                throw new Exception("Cannot add batch: Product not found.");
            }

            await _batchRepository.AddBatchAsync(batch);
            await _batchRepository.SaveChangesAsync();
            await SyncProductTotalQuantityAsync(batch.ProductId);
        }

        public async Task UpdateBatchAsync(Batch batch)
        {
            await _batchRepository.UpdateBatchAsync(batch);
            await SyncProductTotalQuantityAsync(batch.ProductId);
        }

        public async Task DeleteBatchAsync(int id)
        {
            var batch = await _batchRepository.GetBatchByIdAsync(id);
            if (batch != null)
            {
                int productId = batch.ProductId;
                await _batchRepository.DeleteBatchAsync(id);
                await SyncProductTotalQuantityAsync(productId);
            }
        }

        public async Task UpdateQuantity(int batchId, int quantitySold)
        {
            var batch = await _batchRepository.GetBatchByIdAsync(batchId);
            if (batch != null)
            {
                batch.Quantity -= quantitySold;
                if (batch.Quantity < 0) batch.Quantity = 0;

                await _batchRepository.UpdateBatchAsync(batch);
                await SyncProductTotalQuantityAsync(batch.ProductId);
            }
        }

        public async Task SyncProductTotalQuantityAsync(int productId)
        {
            var batches = await _batchRepository.GetBatchesByProductIdAsync(productId);

            await Task.Run(() =>
            {
                var product = _productRepository.GetProductById(productId);
                if (product != null)
                {
                    product.Quantity = batches.Sum(b => b.Quantity);
                    _productRepository.UpdateProduct(product);
                    _productRepository.Save();
                }
            });
        }

        public async Task<IEnumerable<Batch>> GetBatchesByProductIdAsync(int productId)
            => await _batchRepository.GetBatchesByProductIdAsync(productId);

        /// <summary>
        /// FIFO reduction across batches ordered by ExpiryDate (oldest first),
        /// then sync product total quantity.
        /// </summary>
        public async Task ReduceStockFifoAsync(int productId, int quantityToReduce)
        {
            var batches = (await _batchRepository.GetBatchesByProductIdAsync(productId))
                .Where(b => b.Quantity > 0)
                .OrderBy(b => b.ExpiryDate)
                .ToList();

            int remaining = quantityToReduce;

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;

                if (batch.Quantity >= remaining)
                {
                    batch.Quantity -= remaining;
                    remaining = 0;
                }
                else
                {
                    remaining -= batch.Quantity;
                    batch.Quantity = 0;
                }

                await _batchRepository.UpdateBatchAsync(batch);
            }

            await SyncProductTotalQuantityAsync(productId);
        }
        public async Task<Batch?> GetBatchByIdAsync(int id)
        {
            return await _batchRepository.GetBatchByIdAsync(id);
        }
    }
}