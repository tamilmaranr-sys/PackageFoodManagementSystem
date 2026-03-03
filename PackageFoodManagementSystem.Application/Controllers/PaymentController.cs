using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Services.Interfaces;

namespace PackageFoodManagementSystem.Application.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public IActionResult Payment(int orderId)
        {
            if (orderId == 0)
                return BadRequest("OrderId missing from request");

            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpPost]
        public IActionResult Confirm(int orderId, string paymentMethod, string cardNumber)
        {
            // Delegate entire payment flow to the service
            var result = _paymentService.ConfirmPayment(orderId, paymentMethod, cardNumber);

            if (!result.Success)
            {
                // Simulated or real failure → redirect to Failure page (keeps your UX)
                return RedirectToAction("Failure", new { orderId });
            }

            // Success or COD pending → redirect to success page
            return RedirectToAction("Success");
        }

        public IActionResult Success() => View();

        public IActionResult Failure(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}