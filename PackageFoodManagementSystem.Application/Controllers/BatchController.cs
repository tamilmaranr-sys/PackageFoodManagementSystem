//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using PackageFoodManagementSystem.Repository.Models;
//using PackageFoodManagementSystem.Services.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace PackageFoodManagementSystem.Controllers
//{
//    public class BatchController : Controller
//    {
//        private readonly IBatchService _batchService;
//        private readonly IProductService _productService;
//        private readonly IInventoryService _inventoryService;
//        private readonly ICategoryService _categoryService;

//        public BatchController(
//            IBatchService batchService,
//            IProductService productService,
//            IInventoryService inventoryService,
//            ICategoryService categoryService)
//        {
//            _batchService = batchService;
//            _productService = productService;
//            _inventoryService = inventoryService;
//            _categoryService = categoryService;
//        }

//        // GET: /Batch
//        public async Task<IActionResult> Index()
//        {
//            var batches = await _batchService.GetAllBatchesAsync();
//            return View(batches);
//        }

//        // --- AJAX: Products by Category ---
//        [HttpGet]
//        public async Task<JsonResult> GetProductsByCategory(int categoryId)
//        {
//            // 1) Resolve category name for fallback
//            var category = await _categoryService.GetByIdAsync(categoryId);
//            string categoryName = category?.CategoryName ?? string.Empty;

//            // 2) Filter products (either CategoryId or Category name + IsActive)
//            var allProducts = await _productService.GetAllProductsAsync();
//            var products = allProducts
//                .Where(p => p.IsActive == true &&
//                            (p.CategoryId == categoryId ||
//                             string.Equals(p.Category, categoryName, StringComparison.OrdinalIgnoreCase)))
//                .Select(p => new { id = p.ProductId, name = p.ProductName })
//                .ToList();

//            return Json(products);
//        }

//        // GET: /Batch/Create
//        [HttpGet]
//        public async Task<IActionResult> Create()
//        {
//            // Load categories for dropdown
//            var categories = (await _categoryService.GetAllAsync()).ToList();
//            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

//            // Pre-fill defaults
//            var model = new Batch
//            {
//                Categories = categories,
//                ManufactureDate = DateTime.Now,
//                ExpiryDate = DateTime.Now.AddMonths(6)
//            };

//            return View(model);
//        }

//        // POST: /Batch/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Batch batch)
//        {
//            if (ModelState.IsValid)
//            {
//                // 1) Persist batch
//                await _batchService.AddBatchAsync(batch);

//                // 2) Add to inventory
//                await _inventoryService.AddBatchToInventoryAsync(batch);

//                // 3) Sync product total quantity from batches
//                await _batchService.SyncProductTotalQuantityAsync(batch.ProductId);

//                return RedirectToAction(nameof(Index));
//            }

//            // Re-populate categories on validation failure
//            var categories = (await _categoryService.GetAllAsync()).ToList();
//            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

//            return View(batch);
//        }

//        // GET: /Batch/Edit/{id}
//        [HttpGet]
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null) return NotFound();

//            // Load batch via service
//            var batch = await _batchService.GetBatchByIdAsync(id.Value);
//            if (batch == null) return NotFound();

//            // Fallback: derive CategoryId from Product if missing
//            if (batch.CategoryId == null || batch.CategoryId == 0)
//            {
//                var productRef = await _productService.GetProductByIdAsync(batch.ProductId);
//                if (productRef != null)
//                {
//                    batch.CategoryId = productRef.CategoryId;
//                }
//            }

//            // Load categories dropdown
//            var categories = (await _categoryService.GetAllAsync()).ToList();
//            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", batch.CategoryId);

//            // Build products dropdown (filtered)
//            var allProducts = await _productService.GetAllProductsAsync();
//            IEnumerable<Product> filteredProducts = allProducts.Where(p => p.IsActive);

//            if (batch.CategoryId.HasValue && batch.CategoryId.Value > 0)
//            {
//                var cat = categories.FirstOrDefault(c => c.CategoryId == batch.CategoryId);
//                var catName = cat?.CategoryName ?? string.Empty;

//                filteredProducts = filteredProducts.Where(p =>
//                    p.CategoryId == batch.CategoryId ||
//                    string.Equals(p.Category, catName, StringComparison.OrdinalIgnoreCase));
//            }

//            ViewBag.Products = new SelectList(filteredProducts.ToList(), "ProductId", "ProductName", batch.ProductId);

//            return View(batch);
//        }

//        // POST: /Batch/Edit/{id}
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Batch batch)
//        {
//            if (id != batch.BatchId) return NotFound();

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    await _batchService.UpdateBatchAsync(batch);
//                    await _batchService.SyncProductTotalQuantityAsync(batch.ProductId);
//                    return RedirectToAction(nameof(Index));
//                }
//                catch
//                {
//                    ModelState.AddModelError("", "Unable to save changes. Verify Category and Product exist.");
//                }
//            }

//            // Re-populate dropdowns on failure
//            var categories = (await _categoryService.GetAllAsync()).ToList();
//            var category = categories.FirstOrDefault(c => c.CategoryId == batch.CategoryId);
//            string categoryName = category?.CategoryName ?? string.Empty;

//            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", batch.CategoryId);

//            var allProducts = await _productService.GetAllProductsAsync();
//            var filteredProducts = allProducts
//                .Where(p => p.IsActive == true &&
//                            (p.CategoryId == batch.CategoryId ||
//                             string.Equals(p.Category, categoryName, StringComparison.OrdinalIgnoreCase)))
//                .ToList();

//            ViewBag.Products = new SelectList(filteredProducts, "ProductId", "ProductName", batch.ProductId);

//            return View(batch);
//        }

//        // POST: /Batch/Delete/{id}
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int id)
//        {
//            // Service implementation already syncs product total quantity after delete
//            await _batchService.DeleteBatchAsync(id);
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}



using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;

using PackageFoodManagementSystem.Repository.Models;

using PackageFoodManagementSystem.Services.Interfaces;

namespace PackageFoodManagementSystem.Controllers

{

    public class BatchController : Controller

    {

        private readonly IBatchService _batchService;

        private readonly IProductService _productService;

        private readonly IInventoryService _inventoryService;

        private readonly ICategoryService _categoryService;

        public BatchController(

            IBatchService batchService,

            IProductService productService,

            IInventoryService inventoryService,

            ICategoryService categoryService)

        {

            _batchService = batchService;

            _productService = productService;

            _inventoryService = inventoryService;

            _categoryService = categoryService;

        }

        // GET: /Batch

        public async Task<IActionResult> Index()

        {

            var batches = await _batchService.GetAllBatchesAsync();

            return View(batches);

        }

        // --- AJAX: Products by Category ---

        [HttpGet]

        public async Task<JsonResult> GetProductsByCategory(int categoryId)

        {

            // 1) Resolve category name for fallback

            var category = await _categoryService.GetByIdAsync(categoryId);

            string categoryName = category?.CategoryName ?? string.Empty;

            // 2) Filter products (either CategoryId or Category name + IsActive)

            var allProducts = await _productService.GetAllProductsAsync();

            var products = allProducts

                .Where(p => p.IsActive == true &&

                            (p.CategoryId == categoryId ||

                             string.Equals(p.Category, categoryName, StringComparison.OrdinalIgnoreCase)))

                .Select(p => new { id = p.ProductId, name = p.ProductName })

                .ToList();

            return Json(products);

        }

        // GET: /Batch/Create

        [HttpGet]

        public async Task<IActionResult> Create()

        {

            // Load categories for dropdown

            var categories = (await _categoryService.GetAllAsync()).ToList();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            // Pre-fill defaults

            var model = new Batch

            {

                Categories = categories,

                ManufactureDate = DateTime.Now,

                ExpiryDate = DateTime.Now.AddMonths(6)

            };

            return View(model);

        }

        // POST: /Batch/Create

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Batch batch)

        {

            if (ModelState.IsValid)

            {

                // 1) Persist batch

                await _batchService.AddBatchAsync(batch);

                // 2) Add to inventory

                await _inventoryService.AddBatchToInventoryAsync(batch);

                // 3) Sync product total quantity from batches

                await _batchService.SyncProductTotalQuantityAsync(batch.ProductId);

                return RedirectToAction(nameof(Index));

            }

            // Re-populate categories on validation failure

            var categories = (await _categoryService.GetAllAsync()).ToList();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            return View(batch);

        }

        // GET: /Batch/Edit/{id}

        [HttpGet]

        public async Task<IActionResult> Edit(int? id)

        {

            if (id == null) return NotFound();

            // Load batch via service

            var batch = await _batchService.GetBatchByIdAsync(id.Value);

            if (batch == null) return NotFound();

            // Fallback: derive CategoryId from Product if missing

            if (batch.CategoryId == null || batch.CategoryId == 0)

            {

                var productRef = await _productService.GetProductByIdAsync(batch.ProductId);

                if (productRef != null)

                {

                    batch.CategoryId = productRef.CategoryId;

                }

            }

            // Load categories dropdown

            var categories = (await _categoryService.GetAllAsync()).ToList();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", batch.CategoryId);

            // Build products dropdown (filtered)

            var allProducts = await _productService.GetAllProductsAsync();

            IEnumerable<Product> filteredProducts = allProducts.Where(p => p.IsActive);

            if (batch.CategoryId > 0)

            {
                ///
                var cat = categories.FirstOrDefault(c => c.CategoryId == batch.CategoryId);

                var catName = cat?.CategoryName ?? string.Empty;

                filteredProducts = filteredProducts.Where(p =>

                    p.CategoryId == batch.CategoryId ||

                    string.Equals(p.Category, catName, StringComparison.OrdinalIgnoreCase));

            }

            ViewBag.Products = new SelectList(filteredProducts.ToList(), "ProductId", "ProductName", batch.ProductId);

            return View(batch);

        }

        // POST: /Batch/Edit/{id}

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, Batch batch)

        {

            if (id != batch.BatchId) return NotFound();

            if (ModelState.IsValid)

            {

                try

                {

                    await _batchService.UpdateBatchAsync(batch);

                    await _batchService.SyncProductTotalQuantityAsync(batch.ProductId);

                    return RedirectToAction(nameof(Index));

                }

                catch

                {

                    ModelState.AddModelError("", "Unable to save changes. Verify Category and Product exist.");

                }

            }

            // Re-populate dropdowns on failure

            var categories = (await _categoryService.GetAllAsync()).ToList();

            var category = categories.FirstOrDefault(c => c.CategoryId == batch.CategoryId);

            string categoryName = category?.CategoryName ?? string.Empty;

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", batch.CategoryId);

            var allProducts = await _productService.GetAllProductsAsync();

            var filteredProducts = allProducts

                .Where(p => p.IsActive == true &&

                            (p.CategoryId == batch.CategoryId ||

                             string.Equals(p.Category, categoryName, StringComparison.OrdinalIgnoreCase)))

                .ToList();

            ViewBag.Products = new SelectList(filteredProducts, "ProductId", "ProductName", batch.ProductId);

            return View(batch);

        }

        // POST: /Batch/Delete/{id}

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete(int id)

        {

            // Service implementation already syncs product total quantity after delete

            await _batchService.DeleteBatchAsync(id);

            return RedirectToAction(nameof(Index));

        }

    }

}

