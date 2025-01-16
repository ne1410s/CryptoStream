// <copyright file="EncodingExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Encoding;

using CryptoStream.Encoding;
using CryptoStream.Hashing;

/// <summary>
/// Tests for the <see cref="CryptoStream.Encoding.EncodingExtensions"/> class.
/// </summary>
public class EncodingExtensionsTests
{
    [Theory]
    [InlineData(Codec.ByteBase64, "DAAhAAUABgA=")]
    [InlineData(Codec.ByteHex, "0c00210005000600")]
    [InlineData(Codec.CharAscii, "\f\0!\0\u0005\0\u0006\0")]
    [InlineData(Codec.CharUnicode, "\f!\u0005\u0006")]
    [InlineData(Codec.CharUtf8, "\f\0!\0\u0005\0\u0006\0")]
    public void Encode_VaryingCodec_ReturnsExpected(Codec codec, string expected)
    {
        // Arrange
        var source = new byte[] { 12, 0, 33, 0, 5, 0, 6, 0 };

        // Act
        var result = source.Encode(codec);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Encode_BadCodec_ThrowsArgumentException()
    {
        // Arrange
        var source = new byte[] { 12, 0, 33, 0, 5, 0, 6, 0 };

        // Act
        var act = () => source.Encode((Codec)9999);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Bad codec: 9999 (Parameter 'codec')");
    }

    [Fact]
    public void Encode_HexNull_ThrowsArgumentException()
    {
        // Arrange
        var source = (byte[])null!;

        // Act
        var act = () => source.Encode(Codec.ByteHex);

        // Assert
        _ = act.ShouldThrow<ArgumentNullException>();
    }

    [Theory]
    [InlineData(Codec.ByteBase64, "e1b3e2de848872ff73a635b7d7fbb6a0")]
    [InlineData(Codec.ByteHex, "225243157f20e346ebd2771d5c35b887")]
    [InlineData(Codec.CharAscii, "d6b0ab7f1c8ab8f514db9a6d85de160a")]
    [InlineData(Codec.CharUnicode, "6fa7a7be87d9b046e999866d40ff611c")]
    [InlineData(Codec.CharUtf8, "d6b0ab7f1c8ab8f514db9a6d85de160a")]
    public void Decode_VaryingCodec_ReturnsExpectedMd5Hex(Codec codec, string expected)
    {
        // Arrange
        const string source = "abc12345";

        // Act
        var result = source.Decode(codec).Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Decode_BadCodec_ThrowsArgumentException()
    {
        // Arrange
        const string source = "abc12345";

        // Act
        var act = () => source.Decode((Codec)9999);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Bad codec: 9999 (Parameter 'codec')");
    }

    [Fact]
    public void Decode_HexNull_ThrowsArgumentException()
    {
        // Arrange
        const string source = null!;

        // Act
        var act = () => source.Decode(Codec.ByteHex);

        // Assert
        _ = act.ShouldThrow<ArgumentNullException>();
    }
}
