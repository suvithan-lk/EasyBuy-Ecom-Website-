using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/cart")]
public sealed class CartController(IEasyBuyService easyBuyService) : ControllerBase
{
    private readonly IEasyBuyService _easyBuyService = easyBuyService;

    [HttpGet]
    public async Task<ActionResult<CartResponseDto>> GetCart()
    {
        return Ok(await _easyBuyService.GetCart());
    }

    [HttpPost("items")]
    public async Task<IActionResult> UpsertItem([FromBody] UpsertCartItemRequest request)
    {
        var result = await _easyBuyService.UpsertCartItem(request);
        return result.Success && result.Value is not null
            ? Ok(result.Value)
            : BadRequest(new { message = result.Error });
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var result = await _easyBuyService.RemoveCartItem(productId);
        return result.Success && result.Value is not null
            ? Ok(result.Value)
            : BadRequest(new { message = result.Error });
    }
}
