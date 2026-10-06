using System.ComponentModel.DataAnnotations;
using IPVault.API.DTOs.Documents;

namespace IPVault.API.DTOs.Patents;

public class PatentDto
{
    public int PatentId { get; set; }
    public string PatentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public DateTime? GrantDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string PrimaryInventorName { get; set; } = string.Empty;
    public int InventorsCount { get; set; }
    public int DocumentsCount { get; set; }
}

public class PatentDetailDto
{
    public int PatentId { get; set; }
    public string PatentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public DateTime? GrantDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;

    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientCompany { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<PatentInventorDto> Inventors { get; set; } = new();
    public List<PatentDocumentDto> Documents { get; set; } = new();
}

public class CreatePatentDto : IValidatableObject
{
    [Required(ErrorMessage = "Patent number is required.")]
    [StringLength(100, ErrorMessage = "Patent number cannot exceed 100 characters.")]
    public string PatentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Patent title is required.")]
    [StringLength(300, ErrorMessage = "Title cannot exceed 300 characters.")]
    public string Title { get; set; } = string.Empty;

    public string Abstract { get; set; } = string.Empty;

    [Required(ErrorMessage = "Filing date is required.")]
    public DateTime FilingDate { get; set; }

    public DateTime? PublicationDate { get; set; }
    public DateTime? GrantDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    [Required(ErrorMessage = "Patent status is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid patent status.")]
    public int StatusId { get; set; }

    [Required(ErrorMessage = "Client is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid client.")]
    public int ClientId { get; set; }

    [MinLength(1, ErrorMessage = "A patent must have at least one inventor.")]
    public List<int> InventorIds { get; set; } = new();

    public int? PrimaryInventorId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpiryDate.HasValue && ExpiryDate.Value < FilingDate)
        {
            yield return new ValidationResult(
                "Expiry date cannot be before filing date.",
                new[] { nameof(ExpiryDate) }
            );
        }

        if (GrantDate.HasValue && GrantDate.Value < FilingDate)
        {
            yield return new ValidationResult(
                "Grant date cannot be before filing date.",
                new[] { nameof(GrantDate) }
            );
        }
    }
}

public class UpdatePatentDto : IValidatableObject
{
    [Required(ErrorMessage = "Patent number is required.")]
    [StringLength(100, ErrorMessage = "Patent number cannot exceed 100 characters.")]
    public string PatentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Patent title is required.")]
    [StringLength(300, ErrorMessage = "Title cannot exceed 300 characters.")]
    public string Title { get; set; } = string.Empty;

    public string Abstract { get; set; } = string.Empty;

    [Required(ErrorMessage = "Filing date is required.")]
    public DateTime FilingDate { get; set; }

    public DateTime? PublicationDate { get; set; }
    public DateTime? GrantDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    [Required(ErrorMessage = "Patent status is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid patent status.")]
    public int StatusId { get; set; }

    [Required(ErrorMessage = "Client is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid client.")]
    public int ClientId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpiryDate.HasValue && ExpiryDate.Value < FilingDate)
        {
            yield return new ValidationResult(
                "Expiry date cannot be before filing date.",
                new[] { nameof(ExpiryDate) }
            );
        }
    }
}

public class PatentFilterDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public int? StatusId { get; set; }
    public int? ClientId { get; set; }
    public int? Year { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PatentInventorDto
{
    public int InventorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public bool IsPrimaryInventor { get; set; }
}

public class AssignInventorDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid inventor ID.")]
    public int InventorId { get; set; }

    public bool IsPrimaryInventor { get; set; } = false;
}
