using PackageFoodManagementSystem.Repository.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> CreateUserAsync(string name, string mobileNumber, string email, string password, string role, CancellationToken cancellationToken = default);
        Task<UserAuthentication?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserAuthentication?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<UserAuthentication>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task AddUserAsync(UserAuthentication user, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(UserAuthentication user, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(int id, CancellationToken cancellationToken = default); // Unified
        Task<int> CountUsersByRoleAsync(string role, CancellationToken cancellationToken = default);
        Task<UserAuthentication?> AuthenticateAsync(string email, string password);
        Task<(bool Success, string? ErrorMessage)> RegisterUserAsync(UserAuthentication user);
        Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(UserAuthentication user);
        Task<(int TotalCustomers, int TotalStoreManagers, int TotalOrders)> GetAdminDashboardStatsAsync();
    }
}