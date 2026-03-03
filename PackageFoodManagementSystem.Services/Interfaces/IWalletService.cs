using PackageFoodManagementSystem.Repository.Models;

namespace PackageFoodManagementSystem.Services.Interfaces
{
    public interface IWalletService
    {
        Wallet? GetByUserId(int userId);
        Wallet CreateIfMissing(int userId);
        void AddMoney(int userId, decimal amount);
    }
}