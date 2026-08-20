using HotelManagement.API.Services;

namespace HotelManagement.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_GecerliParolaVerildiginde_DuzMetindenFarkliHashDonmeli()
    {
        // Arrange
        const string password = "Test123!";

        // Act
        string hash = PasswordHasher.Hash(password);

        // Assert
        Assert.NotEqual(password, hash);
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void Verify_DogruParolaVerildiginde_TrueDonmeli()
    {
        // Arrange
        const string password = "Test123!";
        string hash = PasswordHasher.Hash(password);

        // Act
        bool result = PasswordHasher.Verify(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_YanlisParolaVerildiginde_FalseDonmeli()
    {
        // Arrange
        const string correctPassword = "Test123!";
        const string wrongPassword = "Yanlis123!";

        string hash = PasswordHasher.Hash(correctPassword);

        // Act
        bool result = PasswordHasher.Verify(
            wrongPassword,
            hash);

        // Assert
        Assert.False(result);
    }
}