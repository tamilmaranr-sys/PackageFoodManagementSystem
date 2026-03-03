using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.DTOs;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Application.Controllers
{
    [Authorize(Roles = "StoreManager")]
    public class StoreManagerController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public StoreManagerController(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Home()
        {
            // Products & stock
            var products = (await _productService.GetAllProductsAsync())?.ToList() ?? new System.Collections.Generic.List<Product>();
            ViewBag.TotalProducts = products.Count;
            ViewBag.AvailableStock = products.Sum(p => p.Quantity);

            // Orders today
            ViewBag.TodayOrders = await _orderService.CountTodayOrdersAsync();

            // Total sales (using order totals to avoid DbContext in controller)
            var totalSales = await _orderService.SumTotalRevenueAsync();
            ViewBag.TotalSales = totalSales.ToString("C");

            // Pending: match your earlier "Placed OR Pending"
            var allOrders = await _orderService.GetOrdersAsync(null);
            ViewBag.PendingOrders = allOrders.Count(o => o.OrderStatus == "Placed" || o.OrderStatus == "Pending");

            // Low stock threshold < 5
            ViewBag.LowStockCount = products.Count(p => p.Quantity < 5);

            return View();
        }

        public IActionResult Profile() => View();

        public IActionResult AddProduct() => View();

        public async Task<IActionResult> OrdersDashboard()
        {
            ViewBag.TodayOrders = await _orderService.CountTodayOrdersAsync();
            var allOrders = await _orderService.GetOrdersAsync(null);
            ViewBag.PendingOrders = allOrders.Count(o => o.OrderStatus == "Pending" || o.OrderStatus == "Placed");
            ViewBag.CompletedOrders = await _orderService.CountCompletedOrdersAsync();
            ViewBag.CancelledOrders = await _orderService.CountCancelledOrdersAsync();
            return View();
        }

        public async Task<IActionResult> Orders(string status)
        {
            var orders = await _orderService.GetOrdersAsync(status);
            return View(orders);
        }

        public IActionResult Inventory() => View();

        public IActionResult Compliance() => View();

        public IActionResult Settings()
        {
            var firstProduct = _productService.GetAllProducts().FirstOrDefault();
            var model = firstProduct ?? new Product { ProductName = "Default Store", Category = "General" };
            return View(model);
        }

        [HttpGet]
        public IActionResult EditProfile(int id) => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(string UserEmail, string UserPhone)
        {
            if (!string.IsNullOrEmpty(UserEmail) && !string.IsNullOrEmpty(UserPhone))
            {
                HttpContext.Session.SetString("UserEmail", UserEmail);
                HttpContext.Session.SetString("UserPhone", UserPhone);
                return RedirectToAction("Settings");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _productService.UpdateProduct(product);
                return RedirectToAction("Settings");
            }
            return View("EditProfile", product);
        }

        public IActionResult Reports()
        {
            // Build last 30 days report through IOrderService (no DbContext here)
            var reportData = _orderService.BuildStoreReport(30);
            return View(reportData);
        }
    }
}