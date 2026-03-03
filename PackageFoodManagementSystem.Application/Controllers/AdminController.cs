using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Application.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly IBatchService _batchService; // Added Batch Service

        public AdminController(
            IOrderService orderService,
            IInventoryService inventoryService,
            IProductService productService,
            IBatchService batchService) // Injected into constructor
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
            _productService = productService;
            _batchService = batchService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(int orderId, string status)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            string adminId = userIdClaim.Value;

            // 1. Update history (Old functionality)
            _orderService.UpdateOrderStatus(orderId, status, adminId, $"Admin updated status to {status}");

            // 2. Reduce Stock (New functionality)
            if (status == "Processing" || status == "Shipped")
            {
                var order = await _orderService.GetOrderByIdAsync(orderId); // Now works!
                if (order != null && order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        // This reduces the Batch AND the Product total quantity
                        await _batchService.UpdateQuantity(item.BatchID, item.Quantity);
                    }
                }
            }

            return RedirectToAction("Dashboard");
        }

        // Helper method to ensure Product.Quantity matches the sum of its Batches
        private async Task UpdateProductTotalQuantity(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null)
            {
                // Get all batches for this specific product
                var batches = await _batchService.GetBatchesByProductIdAsync(productId);

                // Sum the remaining quantities
                product.Quantity = batches.Sum(b => b.Quantity);

                // Save the updated total to the Product table
                await _productService.UpdateProductAsync(product);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminInventory()
        {
            var inventoryData = await _inventoryService.GetInventoryListAsync();
            if (inventoryData == null)
            {
                inventoryData = new List<Inventory>();
            }

            var products = _productService.GetAllProducts();

            var categoryList = products
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();

            ViewBag.TotalCategories = categoryList.Count;
            ViewBag.TotalProducts = products.Count();
            ViewBag.TotalBatches = inventoryData.Select(x => x.BatchId).Distinct().Count();
            ViewBag.OutOfStock = products.Count(p => p.Quantity <= 0);
            ViewBag.CategoryList = categoryList;
            ViewBag.ProductList = new SelectList(products, "ProductId", "ProductName");
            ViewBag.OutOfStockList = products.Where(p => p.Quantity <= 0).ToList();

            return View(inventoryData);
        }
    }
}