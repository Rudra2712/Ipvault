using System.ComponentModel.DataAnnotations;
using IPVault.API.DTOs.Patents;

namespace IPVault.Tests;

public class PatentValidationTests
{
    [Fact]
    public void CreatePatentDto_WhenExpiryIsBeforeFilingDate_ValidationFails()
    {
        // Arrange
        var dto = new CreatePatentDto
        {
            PatentNumber = "IN123",
            Title = "Solar Inverter",
            FilingDate = new DateTime(2025, 1, 1),
            ExpiryDate = new DateTime(2023, 1, 1), // Invalid
            StatusId = 1,
            ClientId = 1,
            InventorIds = new List<int> { 1 }
        };

        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.ErrorMessage != null && r.ErrorMessage.Contains("Expiry date cannot be before"));
    }

    [Fact]
    public void CreatePatentDto_WhenValid_ValidationSucceeds()
    {
        // Arrange
        var dto = new CreatePatentDto
        {
            PatentNumber = "IN123456",
            Title = "Solar Inverter Design",
            FilingDate = new DateTime(2024, 1, 1),
            ExpiryDate = new DateTime(2044, 1, 1),
            StatusId = 1,
            ClientId = 1,
            InventorIds = new List<int> { 1 }
        };

        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }
}
