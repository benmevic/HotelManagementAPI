using System.IdentityModel.Tokens.Jwt;
using HotelManagement.API.Models;
using HotelManagement.API.Services;
using Microsoft.Extensions.Configuration;

namespace HotelManagement.Tests;

public class TokenServiceTests
{
    private static TokenService CreateTokenService()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] =
                "Unit-Test-Icin-En-Az-32-Karakter-Uzunlugunda-Gizli-Anahtar",
            ["Jwt:Issuer"] = "HotelManagementAPI",
            ["Jwt:Audience"] = "HotelManagementAPIUsers",
            ["Jwt:ExpiresInMinutes"] = "60"
        };

        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

        return new TokenService(configuration);
    }

    [Fact]
    public void GenerateToken_GecerliKullaniciVerildiginde_JwtDonmeli()
    {
        // Arrange
        var tokenService = CreateTokenService();

        var user = new User
        {
            Id = 42,
            Name = "Test Kullanici",
            Email = "test@example.com"
        };

        // Act
        string tokenText = tokenService.GenerateToken(user);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(tokenText));

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(tokenText));
    }

    [Fact]
    public void GenerateToken_OlusturulanToken_DogruClaimleriIcermeli()
    {
        // Arrange
        var tokenService = CreateTokenService();

        var user = new User
        {
            Id = 42,
            Name = "Test Kullanici",
            Email = "test@example.com"
        };

        // Act
        string tokenText = tokenService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken token =
            handler.ReadJwtToken(tokenText);

        // Assert
        Assert.Equal("42", token.Subject);

        Assert.Equal(
            "test@example.com",
            token.Claims
                .First(c => c.Type == JwtRegisteredClaimNames.Email)
                .Value);

        Assert.Equal(
            "HotelManagementAPI",
            token.Issuer);

        Assert.Contains(
            "HotelManagementAPIUsers",
            token.Audiences);

        Assert.True(token.ValidTo > DateTime.UtcNow);
    }
}