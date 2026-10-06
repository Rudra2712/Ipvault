using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPVault.API.DTOs.Common;
using IPVault.API.DTOs.PatentStatuses;
using IPVault.API.Services;

namespace IPVault.API.Controllers;

[Authorize]
public class PatentStatusesController : BaseApiController
{
    private readonly IPatentStatusService _statusService;

    public PatentStatusesController(IPatentStatusService statusService)
    {
        _statusService = statusService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PatentStatusDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatuses()
    {
        var statuses = await _statusService.GetStatusesAsync();
        return Ok(ApiResponse<IEnumerable<PatentStatusDto>>.SuccessResult(statuses, "Patent statuses retrieved."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PatentStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusById(int id)
    {
        var status = await _statusService.GetStatusByIdAsync(id);
        return Ok(ApiResponse<PatentStatusDto>.SuccessResult(status, "Patent status details retrieved."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PatentStatusDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateStatus([FromBody] CreatePatentStatusDto dto)
    {
        var created = await _statusService.CreateStatusAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PatentStatusDto>.SuccessResult(created, "Status created.", StatusCodes.Status201Created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PatentStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePatentStatusDto dto)
    {
        var updated = await _statusService.UpdateStatusAsync(id, dto);
        return Ok(ApiResponse<PatentStatusDto>.SuccessResult(updated, "Status updated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStatus(int id)
    {
        await _statusService.DeleteStatusAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null, "Status deleted."));
    }
}
