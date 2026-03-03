using System.Linq;
using PackageFoodManagementSystem.Repository.Data;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;
    public WalletRepository(ApplicationDbContext context) => _context = context;

    public Wallet? GetByUserId(int userId)
        => _context.Wallets.FirstOrDefault(w => w.UserId == userId);

    public void Add(Wallet wallet) => _context.Wallets.Add(wallet);

    public void Update(Wallet wallet) => _context.Wallets.Update(wallet);

    public void Save() => _context.SaveChanges();
}