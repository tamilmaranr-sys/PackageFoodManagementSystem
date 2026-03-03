namespace PackageFoodManagementSystem.Services.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Validates Order & Bill, simulates/records payment, updates Bill status, and advances Order status via IOrderService.
        /// Returns Success=false if payment is declined (e.g., 16 zeros for card).
        /// </summary>
        (bool Success, string? ErrorMessage) ConfirmPayment(int orderId, string paymentMethod, string? cardNumber);
    }
}