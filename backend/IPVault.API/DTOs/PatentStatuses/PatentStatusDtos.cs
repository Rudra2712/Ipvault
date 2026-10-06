using System.ComponentModel.DataAnnotations;

namespace IPVault.API.DTOs.PatentStatuses;

public class PatentStatusDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PatentsCount { get; set; }
}

public class CreatePatentStatusDto
{
    [Required(ErrorMessage = "Status name is required.")]
    [StringLength(100, ErrorMessage = "Status name cannot exceed 100 characters.")]
    public string StatusName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public class UpdatePatentStatusDto
{
    [Required(ErrorMessage = "Status name is required.")]
    [StringLength(100, ErrorMessage = "Status name cannot exceed 100 characters.")]
    public string StatusName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
