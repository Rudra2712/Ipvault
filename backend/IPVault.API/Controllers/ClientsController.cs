using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPVault.API.DTOs.Clients;
using IPVault.API.DTOs.Common;
using IPVault.API.Services;

namespace IPVault.API.Controllers;

[Authorize]
public class ClientsController : BaseApiController
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClientDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClients([FromQuery] string? search)
    {
        var clients = await _clientService.GetClientsAsync(search);
        return Ok(ApiResponse<IEnumerable<ClientDto>>.SuccessResult(clients, "Clients retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientById(int id)
    {
        var client = await _clientService.GetClientByIdAsync(id);
        return Ok(ApiResponse<ClientDto>.SuccessResult(client, "Client details retrieved."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<ClientDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDto dto)
    {
        var created = await _clientService.CreateClientAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ClientDto>.SuccessResult(created, "Client created successfully.", StatusCodes.Status201Created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Attorney")]
    [ProducesResponseType(typeof(ApiResponse<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientDto dto)
    {
        var updated = await _clientService.UpdateClientAsync(id, dto);
        return Ok(ApiResponse<ClientDto>.SuccessResult(updated, "Client updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClient(int id)
    {
        await _clientService.DeleteClientAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null, "Client deleted successfully."));
    }
}
