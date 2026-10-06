using Moq;
using IPVault.API.DTOs.Patents;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Models;
using IPVault.API.Services;

namespace IPVault.Tests;

public class PatentServiceTests
{
    private readonly Mock<IPatentRepository> _patentRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IInventorRepository> _inventorRepoMock;
    private readonly Mock<IPatentStatusRepository> _statusRepoMock;
    private readonly PatentService _patentService;

    public PatentServiceTests()
    {
        _patentRepoMock = new Mock<IPatentRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _inventorRepoMock = new Mock<IInventorRepository>();
        _statusRepoMock = new Mock<IPatentStatusRepository>();

        _patentService = new PatentService(
            _patentRepoMock.Object,
            _clientRepoMock.Object,
            _inventorRepoMock.Object,
            _statusRepoMock.Object
        );
    }

    [Fact]
    public async Task CreatePatent_DuplicatePatentNumber_ThrowsConflictException()
    {
        // Arrange
        var dto = new CreatePatentDto
        {
            PatentNumber = "IN202611009999",
            Title = "AI System",
            FilingDate = DateTime.UtcNow,
            ClientId = 1,
            StatusId = 1,
            InventorIds = new List<int> { 1 }
        };

        _patentRepoMock.Setup(r => r.PatentNumberExistsAsync("IN202611009999", null))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _patentService.CreatePatentAsync(dto));
    }

    [Fact]
    public async Task CreatePatent_ExpiryBeforeFilingDate_ThrowsBadRequestException()
    {
        // Arrange
        var dto = new CreatePatentDto
        {
            PatentNumber = "IN202611001111",
            Title = "Quantum Algorithm",
            FilingDate = new DateTime(2025, 1, 1),
            ExpiryDate = new DateTime(2024, 1, 1), // Invalid expiry before filing
            ClientId = 1,
            StatusId = 1,
            InventorIds = new List<int> { 1 }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _patentService.CreatePatentAsync(dto));
        Assert.Contains("Expiry date cannot be earlier", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreatePatent_NoInventors_ThrowsBadRequestException()
    {
        // Arrange
        var dto = new CreatePatentDto
        {
            PatentNumber = "IN202611001111",
            Title = "Quantum Algorithm",
            FilingDate = new DateTime(2025, 1, 1),
            ClientId = 1,
            StatusId = 1,
            InventorIds = new List<int>() // Empty inventors list
        };

        _patentRepoMock.Setup(r => r.PatentNumberExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);
        _clientRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Client { ClientId = 1 });
        _statusRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new PatentStatus { StatusId = 1 });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _patentService.CreatePatentAsync(dto));
        Assert.Contains("at least one inventor", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeletePatent_NonAdminRole_ThrowsForbiddenException()
    {
        // Arrange
        int patentId = 5;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _patentService.DeletePatentAsync(patentId, "Attorney"));
        Assert.Contains("administrators", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeletePatent_AdminRole_DeletesSuccessfully()
    {
        // Arrange
        int patentId = 5;
        var patent = new Patent { PatentId = patentId, PatentNumber = "IN202611005555" };

        _patentRepoMock.Setup(r => r.GetByIdAsync(patentId))
            .ReturnsAsync(patent);
        _patentRepoMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        await _patentService.DeletePatentAsync(patentId, "Admin");

        // Assert
        _patentRepoMock.Verify(r => r.Delete(patent), Times.Once);
        _patentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPatents_WithFilter_ReturnsPagedResult()
    {
        // Arrange
        var filter = new PatentFilterDto { Search = "Neural", Page = 1, PageSize = 10 };
        var patents = new List<Patent>
        {
            new()
            {
                PatentId = 1,
                PatentNumber = "IN202611001",
                Title = "Neural Core",
                FilingDate = DateTime.UtcNow,
                Client = new Client { ClientName = "ABC Tech" },
                Status = new PatentStatus { StatusName = "Granted" },
                PatentInventors = new List<PatentInventor>
                {
                    new() { Inventor = new Inventor { FirstName = "Alan", LastName = "Turing" }, IsPrimaryInventor = true }
                }
            }
        };

        _patentRepoMock.Setup(r => r.GetPagedPatentsAsync(filter))
            .ReturnsAsync((patents, 1));

        // Act
        var result = await _patentService.GetPatentsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Alan Turing", result.Items.First().PrimaryInventorName);
    }
}
