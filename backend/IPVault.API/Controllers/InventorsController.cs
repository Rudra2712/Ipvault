using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPVault.API.DTOs.Common;
using IPVault.API.DTOs.Inventors;
using IPVault.API.Services;

namespace IPVault.API.Controllers;

[Authorize]
public class InventorsController : BaseApiController
{
    private readonly IInventorService _inventorService;

    public InventorsController(IInventorService inventorService)
    {
        _inventorService = inventorService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InventorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventors([FromQuery] string? search)
    {
        var inventors = await _inventorService.GetInventorsAsync(search);
        return Ok(ApiResponse<IEnumerable<InventorDto>>.SuccessResult(inventors, "Inventors retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InventorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInventorById(int id)
    {
        var inventor = await _inventorService.GetInventorByIdAsync(id);
        return Ok(ApiResponse<InventorDto>.SuccessResult(inventor, "Inventor details retrieved."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<InventorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInventor([FromBody] CreateInventorDto dto)
    {
        var created = await _inventorService.CreateInventorAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<InventorDto>.SuccessResult(created, "Inventor created successfully.", StatusCodes.Status201Created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<InventorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInventor(int id, [FromBody] UpdateInventorDto dto)
    {
        var updated = await _inventorService.UpdateInventorAsync(id, dto);
        return Ok(ApiResponse<InventorDto>.SuccessResult(updated, "Inventor updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInventor(int id)
    {
        await _inventorService.DeleteInventorAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null, "Inventor deleted successfully."));
    }
}
