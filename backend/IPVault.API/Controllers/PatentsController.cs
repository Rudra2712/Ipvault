using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPVault.API.DTOs.Common;
using IPVault.API.DTOs.Documents;
using IPVault.API.DTOs.Patents;
using IPVault.API.Services;

namespace IPVault.API.Controllers;

[Authorize]
public class PatentsController : BaseApiController
{
    private readonly IPatentService _patentService;
    private readonly IDocumentService _documentService;

    public PatentsController(IPatentService patentService, IDocumentService documentService)
    {
        _patentService = patentService;
        _documentService = documentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PatentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatents([FromQuery] PatentFilterDto filter)
    {
        var result = await _patentService.GetPatentsAsync(filter);
        return Ok(ApiResponse<PagedResult<PatentDto>>.SuccessResult(result, "Patents retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PatentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatentById(int id)
    {
        var patent = await _patentService.GetPatentByIdAsync(id);
        return Ok(ApiResponse<PatentDetailDto>.SuccessResult(patent, "Patent details retrieved."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<PatentDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePatent([FromBody] CreatePatentDto dto)
    {
        var created = await _patentService.CreatePatentAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PatentDetailDto>.SuccessResult(created, "Patent created successfully.", StatusCodes.Status201Created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<PatentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatent(int id, [FromBody] UpdatePatentDto dto)
    {
        var updated = await _patentService.UpdatePatentAsync(id, dto);
        return Ok(ApiResponse<PatentDetailDto>.SuccessResult(updated, "Patent updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatent(int id)
    {
        var role = GetCurrentUserRole();
        await _patentService.DeletePatentAsync(id, role);
        return Ok(ApiResponse<object>.SuccessResult(null, "Patent deleted successfully."));
    }

    // --- Patent Inventors ---

    [HttpGet("{patentId:int}/inventors")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PatentInventorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatentInventors(int patentId)
    {
        var inventors = await _patentService.GetPatentInventorsAsync(patentId);
        return Ok(ApiResponse<IEnumerable<PatentInventorDto>>.SuccessResult(inventors, "Patent inventors retrieved."));
    }

    [HttpPost("{patentId:int}/inventors")]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<PatentInventorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignInventor(int patentId, [FromBody] AssignInventorDto dto)
    {
        var result = await _patentService.AssignInventorAsync(patentId, dto);
        return Ok(ApiResponse<PatentInventorDto>.SuccessResult(result, "Inventor assigned to patent."));
    }

    [HttpDelete("{patentId:int}/inventors/{inventorId:int}")]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveInventor(int patentId, int inventorId)
    {
        await _patentService.RemoveInventorAsync(patentId, inventorId);
        return Ok(ApiResponse<object>.SuccessResult(null, "Inventor unassigned from patent."));
    }

    // --- Patent Documents ---

    [HttpGet("{patentId:int}/documents")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PatentDocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatentDocuments(int patentId)
    {
        var docs = await _documentService.GetDocumentsByPatentIdAsync(patentId);
        return Ok(ApiResponse<IEnumerable<PatentDocumentDto>>.SuccessResult(docs, "Patent documents retrieved."));
    }

    [HttpPost("{patentId:int}/documents")]
    [Authorize(Roles = "Admin,Attorney")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<PatentDocumentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(int patentId, [FromForm] UploadDocumentDto dto)
    {
        var email = GetCurrentUserEmail();
        var doc = await _documentService.UploadDocumentAsync(patentId, dto, email);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PatentDocumentDto>.SuccessResult(doc, "Document uploaded successfully.", StatusCodes.Status201Created));
    }
}
