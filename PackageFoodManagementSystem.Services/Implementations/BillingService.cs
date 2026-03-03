using PackageFoodManagementSystem.Repository.Interfaces;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;

namespace PackageFoodManagementSystem.Services.Implementations
{
    public class BillingService : IBillingService
    {
        private readonly IBillRepository _billRepository;
        private readonly IPaymentRepository _paymentRepository;

        public BillingService(IBillRepository billRepository, IPaymentRepository paymentRepository)
        {
            _billRepository = billRepository;
            _paymentRepository = paymentRepository;
        }

        public Bill GenerateBill(int orderId)
        {
            var bill = new Bill
            {
                OrderID = orderId,
                BillDate = DateTime.Now,
                BillingStatus = "Generated"
            };
            _billRepository.AddBill(bill);
            _billRepository.Save();
            return bill;
        }

        // --- NEW METHOD TO CALL FROM ORDER SERVICE ---
        public void UpdateBillStatusByOrder(int orderId, string status)
        {
            var bill = _billRepository.GetBillByOrderId(orderId);
            if (bill != null)
            {
                bill.BillingStatus = status;
                _billRepository.UpdateBill(bill);
                _billRepository.Save();
            }
        }

        public void MakePayment(Payment payment)
        {
            payment.PaymentDate = DateTime.Now;
            payment.PaymentStatus = "Success";
            _paymentRepository.AddPayment(payment);
            _paymentRepository.Save();
        }
    }
}