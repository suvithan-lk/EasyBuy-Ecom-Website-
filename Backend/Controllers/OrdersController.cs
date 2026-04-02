using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api")]
public sealed class OrdersController(IEasyBuyService easyBuyService) : ControllerBase
{
    private readonly IEasyBuyService _easyBuyService = easyBuyService;

    [HttpPost("coupons/validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] CouponValidationRequest request)
    {
        var result = await _easyBuyService.ValidateCoupon(request);
        return result.Success && result.Value is not null
            ? Ok(result.Value)
            : BadRequest(new { message = result.Error });
    }

    [HttpGet("orders")]
    public async Task<ActionResult<OrdersResponseDto>> GetOrders()
    {
        return Ok(await _easyBuyService.GetOrders());
    }

    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var result = await _easyBuyService.GetOrder(id);
        return result.Success && result.Value is not null
            ? Ok(result.Value)
            : NotFound(new { message = result.Error });
    }

    [HttpPost("orders/checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var result = await _easyBuyService.Checkout(request);
        return result.Success && result.Value is not null
            ? Ok(result.Value)
            : BadRequest(new { message = result.Error });
    }
}
