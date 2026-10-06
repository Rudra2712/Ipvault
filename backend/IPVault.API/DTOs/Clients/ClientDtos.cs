using System.ComponentModel.DataAnnotations;

namespace IPVault.API.DTOs.Clients;

public class ClientDto
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int PatentsCount { get; set; }
}

public class CreateClientDto
{
    [Required(ErrorMessage = "Client name is required.")]
    [StringLength(200, ErrorMessage = "Client name cannot exceed 200 characters.")]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class UpdateClientDto
{
    [Required(ErrorMessage = "Client name is required.")]
    [StringLength(200, ErrorMessage = "Client name cannot exceed 200 characters.")]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
