// <copyright file="StringExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.Tests.TestObjects;

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
        roundTrip.ShouldBe(original);
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
        cipher1.ShouldNotBe(cipher2);
        salt1.ShouldBe(salt2);
    }

    [Fact]
    public void Encrypt_CustomProviders_ReturnsExpected()
    {
        // Arrange
        var customCrypto = new TestCrypto();

        // Act
        var cipher = "test".Encrypt("pass", out var salt, customCrypto);

        // Assert
        cipher.ShouldBe("AgQG");
        salt.ShouldBe("AQID");
    }

    [Fact]
    public void Decrypt_CustomProviders_ReturnsExpected()
    {
        // Arrange
        var customCrypto = new TestCrypto();

        // Act
        var plain = "AgQG".Decrypt("pass", "AQID", customCrypto);

        // Assert
        plain.ShouldBe("\u0001\u0003\u0005");
    }

    [Fact]
    public void Hash_WithString_ReturnsExpected()
    {
        // Arrange
        const string str = "hi!";

        // Act
        var result = str.Hash(HashType.Md5).Encode(Codec.ByteBase64);

        // Assert
        result.ShouldBe("r/lxYEdKBW6DjB9yGvAe3w==");
    }
}
