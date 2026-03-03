using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _repo;

    public WalletService(IWalletRepository repo) => _repo = repo;

    public Wallet? GetByUserId(int userId) => _repo.GetByUserId(userId);

    public Wallet CreateIfMissing(int userId)
    {
        var wallet = _repo.GetByUserId(userId);
        if (wallet == null)
        {
            wallet = new Wallet { UserId = userId, Balance = 0m };
            _repo.Add(wallet);
            _repo.Save();
        }
        return wallet;
    }

    public void AddMoney(int userId, decimal amount)
    {
        var wallet = _repo.GetByUserId(userId) ?? CreateIfMissing(userId);
        wallet.Balance += amount;
        _repo.Update(wallet);
        _repo.Save();
    }
}