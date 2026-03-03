using PackageFoodManagementSystem.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;



namespace PackageFoodManagementSystem.Services.Interfaces
{
    public interface IBillingService
    {
        Bill GenerateBill(int orderId);
        void MakePayment(Payment ppayment);
        void UpdateBillStatusByOrder(int orderId, string v);
    }
}
