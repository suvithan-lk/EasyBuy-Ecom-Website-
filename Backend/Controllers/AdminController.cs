using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class AdminController(IEasyBuyService easyBuyService) : ControllerBase
{
    private readonly IEasyBuyService _easyBuyService = easyBuyService;

    [HttpGet("summary")]
    public async Task<ActionResult<AdminSummaryDto>> GetSummary()
    {
        return Ok(await _easyBuyService.GetAdminSummary());
    }
}
