using PackageFoodManagementSystem.Repository.Models;

namespace PackageFoodManagementSystem.Repository.Interfaces
{
    public interface IWalletRepository
    {
        Wallet? GetByUserId(int userId);
        void Add(Wallet wallet);
        void Update(Wallet wallet);
        void Save();
    }
}