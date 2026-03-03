using PackageFoodManagementSystem.DTOs;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IBatchService _batchService;
        private readonly IBillingService _billingService;

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            ICartRepository cartRepository,
            IBatchService batchService,
            IBillingService billingService)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _cartRepository = cartRepository;
            _batchService = batchService;
            _billingService = billingService;
        }

        // ------------------- Basic CRUD -------------------
        public async Task<Order> GetOrderByIdAsync(int orderId)
            => await _orderRepository.GetOrderWithItemsAsync(orderId) ?? _orderRepository.GetOrderById(orderId);

        public IEnumerable<Order> GetAllOrders()
            => _orderRepository.GetAllOrders();

        public Order GetOrderById(int id) => _orderRepository.GetOrderById(id);

        public void PlaceOrder(Order order)
        {
            order.OrderDate = DateTime.Now;
            order.OrderStatus = "Pending";
            _orderRepository.AddOrder(order);
            _orderRepository.Save();
        }

        public void CancelOrder(int orderId)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null) return;

            if (order.OrderStatus == "Preparing" || order.OrderStatus == "Dispatched")
                throw new Exception("Cannot cancel order already in preparation.");

            order.OrderStatus = "Cancelled";
            _orderRepository.UpdateOrder(order);
            _orderRepository.Save();
        }

        public async Task ProcessOrderAsync(int orderId, string status)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null || order.OrderStatus == status) return;

            if (status == "Delivered")
            {
                // 1. Stock reduction
                foreach (var item in order.OrderItems)
                {
                    await _batchService.ReduceStockFifoAsync(item.ProductId, item.Quantity);
                }

                // 2. Update Order Payment Status
                order.PaymentStatus = "Completed";

                // 3. --- FIX: Update Billing Status to Paid ---
                _billingService.UpdateBillStatusByOrder(orderId, "Paid");
            }

            order.OrderStatus = status;
            order.LastUpdateOn = DateTime.Now;
            await _orderRepository.SaveChangesAsync();
        }

        public void UpdateOrderStatus(int orderId, string status, string changedBy, string remarks)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null) return;

            order.OrderStatus = status;
            order.LastUpdateOn = DateTime.Now;

            // If the status is being updated to Confirmed (usually after a successful Card/UPI payment)
            // We set the payment status to Completed immediately.
            if (status == "Confirmed" || status == "Processing")
            {
                order.PaymentStatus = "Completed";
            }

            int.TryParse(changedBy, out int adminUserId);

            var history = new OrderStatusHistory
            {
                OrderID = orderId,
                Status = status,
                ChangedOn = DateTime.Now,
                ChangedBy = adminUserId == 0 ? 1 : adminUserId,
                Remarks = remarks
            };

            _orderRepository.UpdateOrder(order);
            _orderRepository.AddStatusHistoryAsync(history).GetAwaiter().GetResult();
            _orderRepository.Save();
        }

        // ------------------- Create Order from Cart -------------------
        public int CreateOrder(int userId, string address)
        {
            var customers = _customerRepository.GetAllAsync().GetAwaiter().GetResult();
            var customer = customers.FirstOrDefault(c => c.UserId == userId);
            if (customer == null) throw new Exception("Customer profile not found.");

            var cart = _cartRepository.GetActiveCartByUserIdAsync(userId).GetAwaiter().GetResult();
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                throw new Exception("Cart is empty");

            var order = new Order
            {
                CustomerId = customer.CustomerId,
                CreatedByUserID = userId,
                DeliveryAddress = address,
                OrderStatus = "Placed",
                OrderDate = DateTime.Now,
                OrderNumber = "ORD-" + DateTime.Now.Ticks.ToString().Substring(10),
                PaymentStatus = "Pending" // Default until payment gateway or delivery confirms
            };

            _orderRepository.AddOrder(order);
            _orderRepository.Save();

            foreach (var item in cart.CartItems)
            {
                DateTime? expiry = null;
                if (item.BatchID > 0)
                {
                    var batch = _batchService.GetBatchByIdAsync(item.BatchID).GetAwaiter().GetResult();
                    expiry = batch?.ExpiryDate;
                }

                var orderItem = new OrderItem
                {
                    OrderID = order.OrderID,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    BatchID = item.BatchID,
                    ProductNameSnapshot = item.Product.ProductName,
                    ExpiryDate = expiry ?? DateTime.Now.AddMonths(6),
                    CreatedOn = DateTime.Now
                };

                _orderRepository.AddOrderItemAsync(orderItem).GetAwaiter().GetResult();

                if (item.BatchID > 0)
                    _batchService.UpdateQuantity(item.BatchID, item.Quantity).GetAwaiter().GetResult();
                else
                    _batchService.ReduceStockFifoAsync(item.ProductId, item.Quantity).GetAwaiter().GetResult();
            }

            order.TotalAmount = cart.CartItems.Sum(x => x.Quantity * x.Product.Price);
            cart.IsActive = false;

            _orderRepository.Save();
            _cartRepository.UpdateAsync(cart).GetAwaiter().GetResult();
            _cartRepository.SaveChangesAsync().GetAwaiter().GetResult();

            return order.OrderID;
        }

        // ------------------- Dashboard / Queries -------------------
        public Task<int> CountTodayOrdersAsync() => _orderRepository.CountTodayAsync();
        public Task<int> CountPendingOrdersAsync() => _orderRepository.CountPendingAsync();
        public Task<int> CountCompletedOrdersAsync() => _orderRepository.CountCompletedAsync();
        public Task<int> CountCancelledOrdersAsync() => _orderRepository.CountCancelledAsync();

        public Task<List<Order>> GetOrdersAsync(string status = null)
            => _orderRepository.GetOrdersWithCustomerByStatusAsync(status);

        public Task<List<Order>> GetRecentOrdersAsync(int take)
            => _orderRepository.GetRecentOrdersWithCustomerAsync(take);

        public Task<List<Order>> GetAllOrdersWithCustomerAsync()
            => _orderRepository.GetAllWithCustomerAsync();

        public Task<decimal> SumTotalRevenueAsync() => _orderRepository.SumTotalRevenueAsync();

        public async Task<AdminReportDto> BuildAdminReportAsync()
        {
            var lifetimeRevenue = await _orderRepository.SumTotalRevenueAsync();
            var customers = await _customerRepository.GetAllAsync();
            var totalCustomers = customers.Count();

            var totalActiveOrders = (await _orderRepository.GetOrdersWithCustomerByStatusAsync(null))
                .Count(o => o.OrderStatus == "Confirmed" || o.OrderStatus == "Processing");

            var topAgg = await _orderRepository.GetTopCustomersAsync(5);

            var topCustomers = new List<TopCustomerDto>();
            foreach (var item in topAgg)
            {
                var cust = await _customerRepository.GetByIdAsync(item.CustomerId);
                topCustomers.Add(new TopCustomerDto
                {
                    Name = cust?.Name ?? "Unknown",
                    Email = cust?.Email ?? "N/A",
                    OrderCount = item.OrderCount,
                    TotalSpent = item.TotalSpent
                });
            }

            return new AdminReportDto
            {
                LifetimeRevenue = lifetimeRevenue,
                TotalCustomers = totalCustomers,
                TotalActiveOrders = totalActiveOrders,
                TopCustomers = topCustomers
            };
        }

        public StoreReportDto BuildStoreReport(int lastDays)
        {
            var from = DateTime.Now.AddDays(-Math.Abs(lastDays));

            var totalRevenue = _orderRepository
                .GetAllOrders()
                .Where(o => o.OrderDate >= from && o.OrderStatus != "Cancelled")
                .Sum(o => o.TotalAmount);

            var totalOrders = _orderRepository
                .GetAllOrders()
                .Count(o => o.OrderDate != null && o.OrderDate >= from);

            var topProducts = _orderRepository.GetTopProductsSince(from, 5);

            return new StoreReportDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TopProducts = topProducts
            };
        }

        public Task<int> CountOrdersByUserAsync(int userId)
        {
            var count = _orderRepository.GetAllOrders()
                .Count(o => o.CreatedByUserID == userId || o.CustomerId == userId);
            return Task.FromResult(count);
        }

        public Task<List<Order>> GetOrdersByUserAsync(int userId)
        {
            var list = _orderRepository.GetAllOrders()
                .Where(o => o.CreatedByUserID == userId || o.CustomerId == userId)
                .ToList();
            return Task.FromResult(list);
        }
    }
}