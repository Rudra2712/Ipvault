using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IPVault.API.DTOs.Documents;

public class PatentDocumentDto
{
    public int DocumentId { get; set; }
    public int PatentId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
}

public class UploadDocumentDto
{
    [Required(ErrorMessage = "Document type is required.")]
    public string DocumentType { get; set; } = "Application"; // Application, Specification, Drawing, Certificate, LegalDocument, Other

    [Required(ErrorMessage = "File is required.")]
    public IFormFile File { get; set; } = null!;
}
