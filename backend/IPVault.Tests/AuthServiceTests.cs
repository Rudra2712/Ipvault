using Moq;
using IPVault.API.DTOs.Auth;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Models;
using IPVault.API.Services;

namespace IPVault.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtHelper> _jwtHelperMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtHelperMock = new Mock<IJwtHelper>();
        _authService = new AuthService(_userRepoMock.Object, _jwtHelperMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndUser()
    {
        // Arrange
        var rawPassword = "Password123!";
        var user = new User
        {
            UserId = 1,
            FullName = "Test Admin",
            Email = "admin@ipvault.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword),
            Role = "Admin"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("admin@ipvault.com"))
            .ReturnsAsync(user);

        _jwtHelperMock.Setup(j => j.GenerateToken(user))
            .Returns("mock-jwt-token");

        var request = new LoginRequestDto
        {
            Email = "admin@ipvault.com",
            Password = rawPassword
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("mock-jwt-token", result.Token);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal("Admin", result.User.Role);
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsBadRequestException()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "admin@ipvault.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123")
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("admin@ipvault.com"))
            .ReturnsAsync(user);

        var request = new LoginRequestDto
        {
            Email = "admin@ipvault.com",
            Password = "WrongPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task Register_ExistingEmail_ThrowsConflictException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.EmailExistsAsync("existing@ipvault.com"))
            .ReturnsAsync(true);

        var request = new RegisterRequestDto
        {
            FullName = "New User",
            Email = "existing@ipvault.com",
            Password = "Password123!",
            Role = "Attorney"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _authService.RegisterAsync(request));
    }
}
