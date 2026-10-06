using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPVault.API.DTOs.Common;
using IPVault.API.Services;

namespace IPVault.API.Controllers;

[Authorize]
public class DocumentsController : BaseApiController
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("{id:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var (fileBytes, contentType, fileName) = await _documentService.DownloadDocumentAsync(id);
        return File(fileBytes, contentType, fileName);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        await _documentService.DeleteDocumentAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null, "Document deleted successfully."));
    }
}
