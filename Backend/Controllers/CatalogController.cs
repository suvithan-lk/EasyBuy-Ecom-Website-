using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController(IEasyBuyService easyBuyService) : ControllerBase
{
    private readonly IEasyBuyService _easyBuyService = easyBuyService;

    [HttpGet("home")]
    public async Task<ActionResult<HomeResponseDto>> GetHome()
    {
        return Ok(await _easyBuyService.GetHome());
    }

    [HttpGet("products")]
    public async Task<ActionResult<CatalogResponseDto>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? sort)
    {
        return Ok(await _easyBuyService.GetProducts(search, category, sort));
    }

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _easyBuyService.GetProduct(id);
        return result.Success && result.Value is not null
            ? Ok(result.Value)
            : NotFound(new { message = result.Error });
    }
}
