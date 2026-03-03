using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Application.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // CUSTOMER VIEW: Displays all products
        [HttpGet]
        public IActionResult Index()
        {
            var products = _productService.GetMenuForCustomer();
            return View(products);
        }

        // MANAGER VIEW: Shows the 'Add Product' form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = (await _categoryService.GetAllAsync()).ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // If CategoryId not set but Category (name) is provided, try to resolve it
                if ((product.CategoryId == 0 || product.CategoryId == null) && !string.IsNullOrWhiteSpace(product.Category))
                {
                    var categories = await _categoryService.GetAllAsync();
                    var match = categories.FirstOrDefault(c =>
                        string.Equals(c.CategoryName, product.Category, System.StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        product.CategoryId = match.CategoryId;
                    }
                }

                // Persist via service
                _productService.CreateProduct(product);

                return RedirectToAction(nameof(Index));
            }

            // Re-populate dropdown when validation fails
            var allCategories = (await _categoryService.GetAllAsync()).ToList();
            ViewBag.Categories = new SelectList(allCategories, "CategoryId", "CategoryName", product.CategoryId);

            return View(product);
        }
    }
}