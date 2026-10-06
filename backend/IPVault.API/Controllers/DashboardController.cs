using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPVault.API.DTOs.Common;
using IPVault.API.DTOs.Dashboard;
using IPVault.API.Services;

namespace IPVault.API.Controllers;

[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryDto>.SuccessResult(summary, "Dashboard statistics retrieved."));
    }
}
