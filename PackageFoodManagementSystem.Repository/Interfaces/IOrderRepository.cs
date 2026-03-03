using PackageFoodManagementSystem.DTOs;
using PackageFoodManagementSystem.Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Repository.Interfaces
{
    public interface IOrderRepository
    {
        // Existing (keep)
        IEnumerable<Order> GetAllOrders();
        Order GetOrderById(int orderId);
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int orderId);
        void Save();

        // NEW — async query & aggregation APIs
        Task<int> CountTodayAsync();
        Task<int> CountPendingAsync();     // Placed OR Processing
        Task<int> CountCompletedAsync();   // Delivered OR Confirmed
        Task<int> CountCancelledAsync();

        Task<List<Order>> GetOrdersWithCustomerByStatusAsync(string? status);
        Task<List<Order>> GetRecentOrdersWithCustomerAsync(int take);
        Task<List<Order>> GetAllWithCustomerAsync();
        Task<decimal> SumTotalRevenueAsync();

        Task<Order?> GetOrderWithItemsAsync(int orderId);
        Task AddOrderItemAsync(OrderItem item);
        Task AddStatusHistoryAsync(OrderStatusHistory history);
        Task SaveChangesAsync();

        /// <summary>
        /// Returns top N customers by total spend (excluding Cancelled).
        /// Tuple: (CustomerId, OrderCount, TotalSpent)
        /// </summary>
        Task<List<(int CustomerId, int OrderCount, decimal TotalSpent)>> GetTopCustomersAsync(int take);


        // NEW: top products since a date (sync to match your pattern)
        List<TopProductDto> GetTopProductsSince(DateTime from, int take);

    }
}