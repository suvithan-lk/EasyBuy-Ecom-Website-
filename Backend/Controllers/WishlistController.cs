using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/wishlist")]
public sealed class WishlistController(IEasyBuyService easyBuyService) : ControllerBase
{
    private readonly IEasyBuyService _easyBuyService = easyBuyService;

    [HttpGet]
    public async Task<ActionResult<WishlistResponseDto>> GetWishlist()
    {
        return Ok(await _easyBuyService.GetWishlist());
    }

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Toggle(int productId)
    {
        var result = await _easyBuyService.ToggleWishlist(productId);
        return result.Success && result.Value is not null
            ? Ok(result.Value)
            : NotFound(new { message = result.Error });
    }
}
