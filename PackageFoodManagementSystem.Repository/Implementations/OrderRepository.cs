using Microsoft.EntityFrameworkCore;
using PackageFoodManagementSystem.DTOs;
using PackageFoodManagementSystem.Repository.Data;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Repository.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------- Existing (unchanged) --------
        public IEnumerable<Order> GetAllOrders() => _context.Orders.ToList();

        public Order GetOrderById(int orderId)
            => _context.Orders.FirstOrDefault(o => o.OrderID == orderId);

        public void AddOrder(Order order) => _context.Orders.Add(order);

        public void UpdateOrder(Order order) => _context.Orders.Update(order);

        public void DeleteOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null) _context.Orders.Remove(order);
        }

        public void Save() => _context.SaveChanges();

        // -------- NEW async operations --------
        public Task<int> CountTodayAsync()
            => _context.Orders.CountAsync(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == DateTime.Today);

        public Task<int> CountPendingAsync()
            => _context.Orders.CountAsync(o => o.OrderStatus == "Placed" || o.OrderStatus == "Processing");

        public Task<int> CountCompletedAsync()
            => _context.Orders.CountAsync(o => o.OrderStatus == "Delivered" || o.OrderStatus == "Confirmed");

        public Task<int> CountCancelledAsync()
            => _context.Orders.CountAsync(o => o.OrderStatus == "Cancelled");

        public async Task<List<Order>> GetOrdersWithCustomerByStatusAsync(string? status)
        {
            var q = _context.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status)
                {
                    case "Today":
                        q = q.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == DateTime.Today);
                        break;
                    case "Pending":
                        q = q.Where(o => o.OrderStatus == "Placed" || o.OrderStatus == "Processing");
                        break;
                    case "Completed":
                        q = q.Where(o => o.OrderStatus == "Delivered" || o.OrderStatus == "Confirmed");
                        break;
                    case "Cancelled":
                        q = q.Where(o => o.OrderStatus == "Cancelled");
                        break;
                    default:
                        q = q.Where(o => o.OrderStatus == status);
                        break;
                }
            }

            return await q.OrderByDescending(o => o.OrderDate ?? DateTime.MinValue).ToListAsync();
        }

        public Task<List<Order>> GetRecentOrdersWithCustomerAsync(int take)
            => _context.Orders
                       .Include(o => o.Customer)
                       .OrderByDescending(o => o.OrderDate)
                       .Take(take)
                       .ToListAsync();

        public Task<List<Order>> GetAllWithCustomerAsync()
            => _context.Orders
                       .Include(o => o.Customer)
                       .Where(o => o.OrderDate != null)
                       .OrderByDescending(o => o.OrderDate)
                       .ToListAsync();

        public Task<decimal> SumTotalRevenueAsync()
            => _context.Orders.SumAsync(o => o.TotalAmount);

        public Task<Order?> GetOrderWithItemsAsync(int orderId)
            => _context.Orders
                       .Include(o => o.OrderItems)
                       .FirstOrDefaultAsync(o => o.OrderID == orderId);

        public Task AddOrderItemAsync(OrderItem item)
        {
            _context.OrderItems.Add(item);
            return Task.CompletedTask;
        }

        public Task AddStatusHistoryAsync(OrderStatusHistory history)
        {
            _context.OrderStatusHistories.Add(history);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public async Task<List<(int CustomerId, int OrderCount, decimal TotalSpent)>> GetTopCustomersAsync(int take)
        {
            // Exclude cancelled, group by CustomerId
            var data = await _context.Orders
                .Where(o => o.OrderStatus != "Cancelled")
                .GroupBy(o => o.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(take)
                .ToListAsync();

            return data.Select(x => (x.CustomerId, x.OrderCount, x.TotalSpent)).ToList();
        }


        public List<TopProductDto> GetTopProductsSince(DateTime from, int take)
        {
            // Include Order to filter by date range, include Product for name
            var result = _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order != null && oi.Order.OrderDate >= from)
                .GroupBy(oi => oi.Product.ProductName)
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity),
                    // If Subtotal is not persisted correctly, replace with: Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    Revenue = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(take)
                .ToList();

            return result;
        }

    }
}