using PackageFoodManagementSystem.Repository.Models;

namespace PackageFoodManagementSystem.Repository.Interfaces
{
    public interface IPaymentRepository
    {
        void AddPayment(Payment payment);
        Payment? GetPaymentByBillId(int billId);
        Payment? GetPaymentByOrderId(int orderId);
        void UpdatePayment(Payment payment);
        void Save();
    }
}