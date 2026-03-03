using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class StoreManager1Controller : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public StoreManager1Controller(IProductService service, ICategoryService categoryService)
    {
        _productService = service;
        _categoryService = categoryService;
    }

    // 1. MAIN LIST PAGE: This is what opens when clicking "Products"
    [HttpGet]
    public IActionResult Index()
    {
        var products = _productService.GetAllProducts();
        return View(products);
    }

    // Add this so the page opens when you click the button
    [HttpGet]
    public IActionResult AddProduct()
    {
        return View();
    }

    // 2. ADD PRODUCT PAGE: Only shows your beautiful purple form
    //[HttpPost]
    //public async Task<IActionResult> AddProduct(Product product)
    //{
    //    // Look up the ID from the Categories table using the string name
    //    if (!string.IsNullOrWhiteSpace(product.Category) && (product.CategoryId == null || product.CategoryId == 0))
    //    {
    //        var categories = await _categoryService.GetAllAsync();
    //        var categoryData = categories.FirstOrDefault(c =>
    //            string.Equals(c.CategoryName, product.Category, StringComparison.OrdinalIgnoreCase));

    //        if (categoryData != null)
    //        {
    //            // Automatically set the ID so the database stays linked
    //            product.CategoryId = categoryData.CategoryId;
    //        }
    //    }

    //    if (ModelState.IsValid)
    //    {
    //        _productService.CreateProduct(product);
    //        TempData["SuccessMessage"] = "Product added successfully!";
    //        return RedirectToAction("Index");
    //    }

    //    return View(product);
    //}

    // 3. CREATE LOGIC: Handles the Save button
    [HttpPost]
    public async Task<IActionResult> Create(Product product, IFormFile imageFile)
    {
        // 1. Link Category Name to CategoryId automatically
        if (!string.IsNullOrWhiteSpace(product.Category) && (product.CategoryId == null || product.CategoryId == 0))
        {
            var categories = await _categoryService.GetAllAsync();
            var categoryData = categories.FirstOrDefault(c =>
                string.Equals(c.CategoryName, product.Category, StringComparison.OrdinalIgnoreCase));

            if (categoryData != null)
            {
                product.CategoryId = categoryData.CategoryId; // Sets 1 for Veg, 3 for Snacks, etc.
            }
        }

        // 2. Handle the Image File conversion
        if (imageFile != null && imageFile.Length > 0)
        {
            using var ms = new MemoryStream();
            imageFile.CopyTo(ms);
            product.ImageData = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
        }

        // 3. Save everything via service
        if (ModelState.IsValid)
        {
            _productService.CreateProduct(product);
            TempData["SuccessMessage"] = "Product added successfully!";
            return RedirectToAction("Index");
        }

        // If validation fails, stay on the Add page
        return View("AddProduct", product);
    }

    // 4. EDIT PAGE: Shows the form with existing data
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = _productService.GetProductById(id);
        if (product == null) return NotFound();
        return View(product);
    }

    // 5. UPDATE LOGIC
    [HttpPost]
    public IActionResult Edit(Product product, IFormFile? imageFile)
    {
        var productInDb = _productService.GetProductById(product.ProductId);
        if (productInDb == null) return NotFound();

        // Map changes
        productInDb.ProductName = product.ProductName;
        productInDb.Price = product.Price;
        productInDb.Category = product.Category;
        productInDb.CategoryId = product.CategoryId;
        productInDb.IsActive = product.IsActive;
        productInDb.Quantity = product.Quantity;

        if (imageFile != null && imageFile.Length > 0)
        {
            using var ms = new MemoryStream();
            imageFile.CopyTo(ms);
            productInDb.ImageData = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
        }

        if (ModelState.IsValid)
        {
            _productService.UpdateProduct(productInDb);
            TempData["SuccessMessage"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    // 6. DELETE LOGIC: Triggered by the trash icon
    [HttpPost]
    public IActionResult Delete(int id)
    {
        _productService.DeleteProduct(id);
        TempData["DeleteMessage"] = "Product deleted successfully!";
        return RedirectToAction("Index");
    }
}