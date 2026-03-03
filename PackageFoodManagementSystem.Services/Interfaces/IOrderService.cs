using PackageFoodManagementSystem.DTOs;
using PackageFoodManagementSystem.Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOrderService
{
    IEnumerable<Order> GetAllOrders();
    Order GetOrderById(int orderId);
    Task<Order> GetOrderByIdAsync(int orderId);
    void PlaceOrder(Order order);
    void UpdateOrderStatus(int orderId, string status, string changedBy, string remarks = "");
    void CancelOrder(int orderId);
    int CreateOrder(int userId, string deliveryAddress);

    // NEW: Dashboard counts
    Task<int> CountTodayOrdersAsync();
    Task<int> CountPendingOrdersAsync();    // Placed OR Processing
    Task<int> CountCompletedOrdersAsync();  // Delivered OR Confirmed
    Task<int> CountCancelledOrdersAsync();

    // NEW: Order queries (include customers)
    Task<List<Order>> GetOrdersAsync(string status = null); // honors same filters as before
    Task<List<Order>> GetRecentOrdersAsync(int take);        // includes Customer
    Task<List<Order>> GetAllOrdersWithCustomerAsync();       // latest first

    // NEW: Aggregations
    Task<decimal> SumTotalRevenueAsync();

    // NEW: Composite operations
    Task ProcessOrderAsync(int orderId, string status);      // includes FIFO stock reduction when Delivered
    Task<AdminReportDto> BuildAdminReportAsync();            // build report with TopCustomers etc.


    // NEW: Store Manager report (last N days)
    StoreReportDto BuildStoreReport(int lastDays);

    Task<int> CountOrdersByUserAsync(int userId);
    Task<System.Collections.Generic.List<Order>> GetOrdersByUserAsync(int userId);

}