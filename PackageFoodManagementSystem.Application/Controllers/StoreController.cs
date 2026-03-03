using Microsoft.AspNetCore.Mvc;

namespace PackageFoodManagementSystem.Controllers
{
    public class StoreController : Controller
    {
        public IActionResult AddProduct()
        {
            return View();
        }
        public IActionResult Report()
        {
            return View();
        }
    }
}