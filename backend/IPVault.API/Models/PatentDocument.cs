namespace IPVault.API.Models;

public class PatentDocument
{
    public int DocumentId { get; set; }
    public int PatentId { get; set; }
    public Patent? Patent { get; set; }

    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "Application"; // Application, Specification, Drawing, Certificate, LegalDocument, Other
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedBy { get; set; } = string.Empty;
}
