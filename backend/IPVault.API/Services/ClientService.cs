using IPVault.API.DTOs.Clients;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Models;

namespace IPVault.API.Services;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetClientsAsync(string? search);
    Task<ClientDto> GetClientByIdAsync(int id);
    Task<ClientDto> CreateClientAsync(CreateClientDto dto);
    Task<ClientDto> UpdateClientAsync(int id, UpdateClientDto dto);
    Task DeleteClientAsync(int id);
}

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<ClientDto>> GetClientsAsync(string? search)
    {
        var clients = await _clientRepository.SearchClientsAsync(search);
        return clients.Select(MapToClientDto);
    }

    public async Task<ClientDto> GetClientByIdAsync(int id)
    {
        var client = await _clientRepository.GetClientWithPatentsAsync(id);
        if (client == null)
        {
            throw new NotFoundException($"Client with ID {id} was not found.");
        }

        return MapToClientDto(client);
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientDto dto)
    {
        var client = new Client
        {
            ClientName = dto.ClientName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Company = dto.Company?.Trim() ?? string.Empty,
            Address = dto.Address?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();

        return MapToClientDto(client);
    }

    public async Task<ClientDto> UpdateClientAsync(int id, UpdateClientDto dto)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
        {
            throw new NotFoundException($"Client with ID {id} was not found.");
        }

        client.ClientName = dto.ClientName.Trim();
        client.Email = dto.Email.Trim().ToLower();
        client.Phone = dto.Phone?.Trim() ?? string.Empty;
        client.Company = dto.Company?.Trim() ?? string.Empty;
        client.Address = dto.Address?.Trim() ?? string.Empty;
        client.UpdatedAt = DateTime.UtcNow;

        _clientRepository.Update(client);
        await _clientRepository.SaveChangesAsync();

        return MapToClientDto(client);
    }

    public async Task DeleteClientAsync(int id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
        {
            throw new NotFoundException($"Client with ID {id} was not found.");
        }

        // Business Rule 5: Client cannot be deleted if patents are associated with the client
        var hasPatents = await _clientRepository.HasAssociatedPatentsAsync(id);
        if (hasPatents)
        {
            throw new BadRequestException("Cannot delete client because there are active patents associated with this client.");
        }

        _clientRepository.Delete(client);
        await _clientRepository.SaveChangesAsync();
    }

    private static ClientDto MapToClientDto(Client client)
    {
        return new ClientDto
        {
            ClientId = client.ClientId,
            ClientName = client.ClientName,
            Email = client.Email,
            Phone = client.Phone,
            Company = client.Company,
            Address = client.Address,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
            PatentsCount = client.Patents?.Count ?? 0
        };
    }
}
