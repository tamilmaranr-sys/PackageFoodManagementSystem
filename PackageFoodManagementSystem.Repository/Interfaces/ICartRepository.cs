using PackageFoodManagementSystem.Repository.Models;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Repository.Interfaces
{
    public interface ICartRepository
    {
        // Reads
        Task<Cart?> GetActiveCartByUserIdAsync(int userAuthId);
        Task<Cart?> GetByIdAsync(int cartId);

        // Writes
        Task AddAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task AddItemAsync(CartItem cartItem);
        Task RemoveItemAsync(CartItem cartItem);

        // Unit of work
        Task SaveChangesAsync();
    }
}
