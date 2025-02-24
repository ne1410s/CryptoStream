﻿// <copyright file="DefaultKeyDeriverTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Keying;

using CryptoStream.Encoding;
using CryptoStream.Keying;

/// <summary>
/// Tests for the <see cref="DefaultKeyDeriver"/>. class.
/// </summary>
public class DefaultKeyDeriverTests
{
    [Fact]
    public void DeriveKey_DifferentOrderHashes_ProduceSameResult()
    {
        // Arrange
        const string seed = "x";
        const string expected = "3CVz+djWBRFxIVQRMMPYMPCIClI=";
        var sut = new DefaultKeyDeriver();
        var hash1 = new byte[] { 1, 2, 3 };
        var hash2 = new byte[] { 4, 5, 6 };

        // Act
        var key1 = sut.DeriveKey(seed, hash1, hash2).Encode(Codec.ByteBase64);
        var key2 = sut.DeriveKey(seed, hash2, hash1).Encode(Codec.ByteBase64);

        // Assert
        key1.ShouldBe(key2);
        key1.ShouldBe(expected);
    }

    [Fact]
    public void DeriveCryptoKey_WithValues_ProducesExpected()
    {
        // Arrange
        var key = new byte[] { 1, 2, 3 };
        var salt = new byte[] { 4, 5, 6 };
        var pepper = new byte[] { 7, 8, 9 };
        var sut = new DefaultKeyDeriver();

        // Act
        var cryptoKeyHex = sut.DeriveCryptoKey(key, salt, pepper).Encode(Codec.ByteHex);

        // Assert
        cryptoKeyHex.ShouldBe("d03434869fc5797972b0b602daab986a4a89d4827ea0a884058ee4fc46dba345");
    }
}
