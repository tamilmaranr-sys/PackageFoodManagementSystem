using PackageFoodManagementSystem.Repository.Implementations;

using PackageFoodManagementSystem.Repository.Interfaces;

using PackageFoodManagementSystem.Repository.Models;

using PackageFoodManagementSystem.Services.Interfaces;

using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;

public class CartService : ICartService

{

    private readonly ICartRepository _cartRepository;

    private readonly IBatchRepository _batchRepository;

    public CartService(ICartRepository cartRepository, IBatchRepository batchRepository)

    {

        _cartRepository = cartRepository;

        _batchRepository = batchRepository;

    }

    // Keep sync signature for backward compatibility (call async underneath)

    public void AddItem(int userAuthId, int productId)

        => AddItemAsync(userAuthId, productId).GetAwaiter().GetResult();

    public void DecreaseItem(int userAuthId, int productId)

        => DecreaseItemAsync(userAuthId, productId).GetAwaiter().GetResult();

    public void Remove(int userAuthId, int productId)

        => RemoveAsync(userAuthId, productId).GetAwaiter().GetResult();

    public Cart GetActiveCart(int userAuthId)

        => GetActiveCartAsync(userAuthId).GetAwaiter().GetResult();

    public async Task<Cart> GetActiveCartAsync(int userAuthId)

    {

        // Get or create active cart

        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userAuthId);

        if (cart == null)

        {

            cart = new Cart

            {

                UserAuthenticationId = userAuthId,

                IsActive = true,

                CreatedAt = DateTime.Now,

                CartItems = new List<CartItem>()

            };

            await _cartRepository.AddAsync(cart);

            await _cartRepository.SaveChangesAsync();

            // Reload to ensure CartId and navigations are populated

            cart = await _cartRepository.GetActiveCartByUserIdAsync(userAuthId) ?? cart;

        }

        return cart;

    }

    public string? GetCartByUserId(int userId) => null; // preserved

    public void AddToCart(int userAuthId, int productId) => AddItem(userAuthId, productId);

    // --------------- Internals (async) ---------------

    private async Task AddItemAsync(int userAuthId, int productId)

    {

        var cart = await GetActiveCartAsync(userAuthId);

        // 3. Find the oldest available batch for this product (FIFO)

        var availableBatches = await _batchRepository.GetBatchesByProductIdAsync(productId);

        var bestBatch = availableBatches

            .Where(b => b.Quantity > 0)

            .OrderBy(b => b.ExpiryDate)

            .FirstOrDefault();

        var item = cart.CartItems.FirstOrDefault(x => x.ProductId == productId);

        if (item == null)

        {

            item = new CartItem

            {

                CartId = cart.CartId,

                ProductId = productId,

                Quantity = 1,

                // 4. Assign the real BatchID here

                BatchID = bestBatch?.BatchId ?? 0

            };

            await _cartRepository.AddItemAsync(item);

        }

        else

        {

            item.Quantity++;

            // Optional: Update BatchID if the current one is 0

            if (item.BatchID == 0 && bestBatch != null)

            {

                item.BatchID = bestBatch.BatchId;

            }

            await _cartRepository.UpdateAsync(cart);

        }

        await _cartRepository.SaveChangesAsync();

    }

    private async Task DecreaseItemAsync(int userAuthId, int productId)

    {

        var cart = await GetActiveCartAsync(userAuthId);

        var item = cart.CartItems.FirstOrDefault(x => x.ProductId == productId);

        if (item == null) return;

        item.Quantity--;

        if (item.Quantity <= 0)

        {

            await _cartRepository.RemoveItemAsync(item);

        }

        else

        {

            await _cartRepository.UpdateAsync(cart);

        }

        await _cartRepository.SaveChangesAsync();

    }

    private async Task RemoveAsync(int userAuthId, int productId)

    {

        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userAuthId);

        if (cart == null) return;

        var item = cart.CartItems.FirstOrDefault(x => x.ProductId == productId);

        if (item != null)

        {

            await _cartRepository.RemoveItemAsync(item);

            await _cartRepository.SaveChangesAsync();

        }

    }

}
