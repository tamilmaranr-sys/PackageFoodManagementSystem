using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;

namespace PackageFoodManagementSystem.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IBillRepository _billRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderService _orderService;

        public PaymentService(
            IBillRepository billRepository,
            IPaymentRepository paymentRepository,
            IOrderService orderService)
        {
            _billRepository = billRepository;
            _paymentRepository = paymentRepository;
            _orderService = orderService;
        }

        public (bool Success, string? ErrorMessage) ConfirmPayment(int orderId, string paymentMethod, string? cardNumber)
        {
            // 1) Validate order exists
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return (false, "Order not found");

            // 2) Validate bill exists
            var bill = _billRepository.GetBillByOrderId(orderId);
            if (bill == null)
                return (false, "Bill not found for this order.");

            // 3) Simulated failure logic (16 zeros for card)
            if (string.Equals(paymentMethod, "Card", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(cardNumber))
            {
                var clean = cardNumber.Replace(" ", "");
                if (clean == "0000000000000000")
                {
                    _orderService.UpdateOrderStatus(orderId, "Payment Failed", "System", "Card declined by bank.");
                    return (false, "Card declined by bank.");
                }
            }

            // 4) Compute statuses
            var paymentStatus = string.Equals(paymentMethod, "COD", StringComparison.OrdinalIgnoreCase) ? "Pending" : "Success";
            var nextOrderStatus = string.Equals(paymentMethod, "COD", StringComparison.OrdinalIgnoreCase) ? "Placed" : "Confirmed";

            // 5) Create Payment row
            var payment = new Payment
            {
                BillID = bill.BillID,
                OrderID = orderId,
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentStatus,
                PaymentDate = DateTime.Now,
                TransactionReference = Guid.NewGuid().ToString(),
                AmountPaid = bill.FinalAmount,
                GatewayResponse = paymentStatus == "Success" ? "APPROVED: AuthCode_" + Guid.NewGuid().ToString().Substring(0, 8) : "WAITING_FOR_COD"
            };
            _paymentRepository.AddPayment(payment);

            // 6) Update bill as Paid for successful online payment
            if (string.Equals(paymentStatus, "Success", StringComparison.OrdinalIgnoreCase))
            {
                bill.BillingStatus = "Paid";
                _billRepository.UpdateBill(bill);
            }

            // Persist all changes (Payment + Bill)
            _paymentRepository.Save();
            _billRepository.Save();

            // 7) Advance order status & add status history through OrderService
            _orderService.UpdateOrderStatus(
                orderId,
                nextOrderStatus,
                "System_Auto_Payment",
                $"Payment processed via {paymentMethod}"
            );

            return (true, null);
        }
    }
}