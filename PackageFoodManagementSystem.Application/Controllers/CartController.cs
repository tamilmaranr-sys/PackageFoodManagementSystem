using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackageFoodManagementSystem.Application.DTOs;
using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
[Route("Cart")]
public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // ================== CART PAGE ==================
    [HttpGet("MyBasket")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> MyBasket()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var cart = await _cartService.GetActiveCartAsync(userId);

        // Safety: if null (shouldn't be, service creates it), construct empty shell
        if (cart == null)
        {
            cart = new Cart
            {
                UserAuthenticationId = userId,
                CartItems = new System.Collections.Generic.List<CartItem>()
            };
        }

        return View("MyBasket", cart);
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromQuery] int productId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // 1) Update via service
        _cartService.AddItem(userId, productId);

        // 2) Fetch updated data for JSON response via service
        var cart = await _cartService.GetActiveCartAsync(userId);
        var item = cart?.CartItems.FirstOrDefault(x => x.ProductId == productId);
        var totalCount = cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;

        return Json(new
        {
            success = true,
            newQty = item?.Quantity ?? 0,
            cartCount = totalCount
        });
    }

    [HttpPost("Decrease")]
    public async Task<IActionResult> Decrease([FromQuery] int productId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // 1) Update via service
        _cartService.DecreaseItem(userId, productId);

        // 2) Fetch updated data for JSON response via service
        var cart = await _cartService.GetActiveCartAsync(userId);
        var item = cart?.CartItems.FirstOrDefault(x => x.ProductId == productId);
        var totalCount = cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;

        return Json(new
        {
            success = true,
            newQty = item?.Quantity ?? 0,
            cartCount = totalCount
        });
    }

    [HttpGet("GetItemQty")]
    public async Task<IActionResult> GetItemQty(int productId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var cart = await _cartService.GetActiveCartAsync(userId);
        if (cart == null) return Json(0);

        var item = cart.CartItems.FirstOrDefault(x => x.ProductId == productId);
        return Json(item?.Quantity ?? 0);
    }

    [HttpPost("Remove")]
    public IActionResult Remove([FromBody] CartRequest request)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _cartService.Remove(userId, request.ProductId);
        return Ok();
    }

    [HttpGet("GetTotalItems")]
    public async Task<IActionResult> GetTotalItems()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Json(0);

        int userId = int.Parse(claim.Value);

        var cart = await _cartService.GetActiveCartAsync(userId);
        var totalCount = cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;

        return Json(totalCount);
    }
}