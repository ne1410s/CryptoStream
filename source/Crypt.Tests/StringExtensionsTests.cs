using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.Tests.TestObjects;

namespace Crypt.Tests;

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
        var customCrypto = new TestCrypto();

        // Act
        var cipher = "test".Encrypt("pass", out var salt, customCrypto);

        // Assert
        cipher.Should().Be("AgQG");
        salt.Should().Be("AQID");
    }

    [Fact]
    public void Hash_WithString_ReturnsExpected()
    {
        // Arrange
        var str = "hi!";

        // Act
        var result = str.Hash(HashType.Md5).Encode(Codec.ByteBase64);

        // Assert
        result.Should().Be("r/lxYEdKBW6DjB9yGvAe3w==");
    }
}
