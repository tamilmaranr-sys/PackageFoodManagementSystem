using System.Linq;
using PackageFoodManagementSystem.Repository.Data;
using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;

namespace PackageFoodManagementSystem.Repository.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddPayment(Payment payment)
        {
            _context.Payment.Add(payment);
        }

        public Payment? GetPaymentByBillId(int billId)
        {
            return _context.Payment.FirstOrDefault(p => p.BillID == billId);
        }

        public Payment? GetPaymentByOrderId(int orderId)
        {
            return _context.Payment.FirstOrDefault(p => p.OrderID == orderId);
        }

        public void UpdatePayment(Payment payment)
        {
            _context.Payment.Update(payment);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}