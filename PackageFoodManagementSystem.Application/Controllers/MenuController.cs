using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Application.Controllers
{
    public class MenuController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        // Updated constructor to include ICartService
        public MenuController(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }



        // GET: Menu/Index
        [Authorize(Roles = "User")]
        public IActionResult Index(string? category, string? searchTerm)

{

     // 1. Fetch all products

     var products = _productService.GetAllProducts();
 
     // 2. Filter by Search Term (New Logic)

     if (!string.IsNullOrEmpty(searchTerm))

     {

         products = products.Where(p =>

             p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||

             (p.Category != null && p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))

         ).ToList();
 
         ViewBag.CurrentSearch = searchTerm;

     }
 
     // 3. Filter by Category

     if (!string.IsNullOrEmpty(category))

     {

         products = products.Where(p => p.Category == category).ToList();

         ViewBag.SelectedCategory = category;

     }
 
     return View(products);

}
 

        // GET: Menu/Details/5
        public IActionResult Details(int id)
        {
            var product = _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Helper method to get the logged-in User ID
        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int id) ? id : 0;
        }


    }
}