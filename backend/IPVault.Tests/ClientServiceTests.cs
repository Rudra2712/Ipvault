using Moq;
using IPVault.API.DTOs.Clients;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Models;
using IPVault.API.Services;

namespace IPVault.Tests;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly ClientService _clientService;

    public ClientServiceTests()
    {
        _clientRepoMock = new Mock<IClientRepository>();
        _clientService = new ClientService(_clientRepoMock.Object);
    }

    [Fact]
    public async Task CreateClient_ValidDto_ReturnsCreatedClient()
    {
        // Arrange
        var dto = new CreateClientDto
        {
            ClientName = "Acme Corp",
            Email = "legal@acme.com",
            Company = "Acme Corporation",
            Phone = "1234567890",
            Address = "123 Tech Park"
        };

        _clientRepoMock.Setup(r => r.AddAsync(It.IsAny<Client>()))
            .Returns(Task.CompletedTask);
        _clientRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _clientService.CreateClientAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.ClientName, result.ClientName);
        Assert.Equal(dto.Email, result.Email);
    }

    [Fact]
    public async Task DeleteClient_WhenClientHasPatents_ThrowsBadRequestException()
    {
        // Arrange
        int clientId = 10;
        var client = new Client { ClientId = clientId, ClientName = "Test Client" };

        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.HasAssociatedPatentsAsync(clientId))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _clientService.DeleteClientAsync(clientId));
        Assert.Contains("patents associated", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteClient_WhenClientHasNoPatents_SuccessfullyDeletes()
    {
        // Arrange
        int clientId = 10;
        var client = new Client { ClientId = clientId, ClientName = "Test Client" };

        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.HasAssociatedPatentsAsync(clientId))
            .ReturnsAsync(false);
        _clientRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        await _clientService.DeleteClientAsync(clientId);

        // Assert
        _clientRepoMock.Verify(r => r.Delete(client), Times.Once);
        _clientRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
