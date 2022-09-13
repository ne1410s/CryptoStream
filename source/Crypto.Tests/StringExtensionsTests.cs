using Crypto.Tests.TestObjects;

namespace Crypto.Tests;

/// <summary>
/// Tests for the <see cref="StringExtensions"/> class.
/// </summary>
public class StringExtensionsTests
{
    [Fact]
    public void EncryptDecrypt_WithString_ReturnsOriginal()
    {
        // Arrange
        const string original = "hello world";
        const string password = "password1";

        // Act
        var cipher = original.Encrypt(password, out var salt);
        var roundTrip = cipher.Decrypt(password, salt);

        // Assert
        roundTrip.Should().Be(original);
    }

    [Fact]
    public void Encrypt_Twice_SameSaltDifferentCipher()
    {
        // Arrange
        const string original = "hello";
        const string password = "pass";

        // Act
        var cipher1 = original.Encrypt(password, out var salt1);
        var cipher2 = original.Encrypt(password, out var salt2);

        // Assert
        cipher1.Should().NotBe(cipher2);
        salt1.Should().Be(salt2);
    }

    [Fact]
    public void Encrypt_CustomProviders_ReturnsExpected()
    {
        // Arrange
        var customDeriver = new TestKeyDeriver();
        var customCrypto = new TestCrypto();

        // Act
        var cipher = "test".Encrypt("pass", out var salt, customCrypto, customDeriver);

        // Assert
        cipher.Should().Be("AgQG");
        salt.Should().Be("AQID");
    }
}
