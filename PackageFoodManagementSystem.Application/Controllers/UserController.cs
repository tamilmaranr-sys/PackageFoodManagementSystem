using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PackagedFoodFrontend.Controllers
{
    public class UserController : Controller
    {
        private readonly ICustomerAddressService _addressService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IWalletService _walletService;

        public UserController(
            ICustomerAddressService addressService,
            IOrderService orderService,
            ICustomerService customerService,
            IWalletService walletService)
        {
            _addressService = addressService;
            _orderService = orderService;
            _customerService = customerService;
            _walletService = walletService;
        }

        #region Dashboard & Profile

        public async Task<IActionResult> Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("SignIn", "Home");

            // Meta Data
            ViewBag.FullName = HttpContext.Session.GetString("UserName");
            ViewBag.Email = HttpContext.Session.GetString("UserEmail");
            ViewBag.Phone = HttpContext.Session.GetString("UserPhone");

            // Orders count (CreatedByUserID)
            ViewBag.TotalOrders = await _orderService.CountOrdersByUserAsync(userId.Value);

            // Wallet (create if missing)
            var wallet = _walletService.GetByUserId(userId.Value) ?? _walletService.CreateIfMissing(userId.Value);
            ViewBag.WalletBalance = wallet?.Balance ?? 0m;

            return View();
        }

        public IActionResult EditProfile() => View(GetUserFromSession());

        [HttpPost]
        public IActionResult UpdateProfile(UserAuthentication model)
        {
            HttpContext.Session.SetString("UserName", model.Name ?? "Guest");
            HttpContext.Session.SetString("UserPhone", model.MobileNumber ?? "");
            // In a real app, call: _userService.UpdateProfile(model);
            return Json(new { success = true });
        }

        public IActionResult MyBasket() => View(GetUserFromSession());
        public IActionResult SmartBasket() => View(GetUserFromSession());

        public async Task<IActionResult> MyOrders()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("SignIn", "Home");

            // Orders for this user (CreatedByUserID or mapped CustomerId)
            var orders = await _orderService.GetOrdersByUserAsync(userId.Value);
            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            try
            {
                _orderService.CancelOrder(orderId);
                return Json(new { success = true, message = "Order cancelled successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public IActionResult Payment() => View();


        

      

        #endregion

        #region Address Management

        public async Task<IActionResult> DeliveryAddress()
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null) return RedirectToAction("SignIn", "Home");

            // Get Customer by UserId
            var customer = await _customerService.GetByUserIdAsync(sessionUserId.Value);
            if (customer == null) return View(new System.Collections.Generic.List<CustomerAddress>());

            // Fetch & filter addresses
            var all = await _addressService.GetAllAsync();
            var userAddresses = all.Where(x => x.CustomerId == customer.CustomerId).ToList();

            return View(userAddresses);
        }

        [HttpGet]
        public IActionResult AddAddress()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("SignIn", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(CustomerAddress address)
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (!sessionUserId.HasValue) return RedirectToAction("SignIn", "Home");

            // Find Customer by UserId
            var customer = await _customerService.GetByUserIdAsync(sessionUserId.Value);
            if (customer == null)
            {
                ModelState.AddModelError("", "Customer profile not found.");
                return View(address);
            }

            // Assign FK + clear validation for unmapped fields (as before)
            address.CustomerId = customer.CustomerId;

            ModelState.Remove("CustomerId");
            ModelState.Remove("Customer");
            ModelState.Remove("State");
            ModelState.Remove("Country");

            if (ModelState.IsValid)
            {
                try
                {
                    await _addressService.AddAsync(address);
                    TempData["Message"] = "Address saved successfully!";
                    return RedirectToAction("DeliveryAddress");
                }
                catch (Exception ex)
                {
                    var inner = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", "Database Error: " + inner);
                }
            }

            return View(address);
        }

        public async Task<IActionResult> DeleteAddress(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("SignIn", "Home");

            await _addressService.DeleteAsync(id);
            TempData["Message"] = "Address removed!";
            return RedirectToAction("DeliveryAddress");
        }

        #endregion

        #region Helpers & Auth

        private UserAuthentication GetUserFromSession()
        {
            return new UserAuthentication
            {
                Id = HttpContext.Session.GetInt32("UserId") ?? 0,
                Name = HttpContext.Session.GetString("UserName") ?? "Guest",
                Email = HttpContext.Session.GetString("UserEmail") ?? "",
                MobileNumber = HttpContext.Session.GetString("UserPhone") ?? ""
            };
        }

        public IActionResult EmailAddress() => View(GetUserFromSession());
        public IActionResult ContactUs() => View();

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Welcome", "Home");
        }

        #endregion
    }
}