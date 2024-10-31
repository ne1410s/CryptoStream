// <copyright file="BlockWriteStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.Streams;

/// <summary>
/// Tests for the <see cref="BlockWriteStream"/>.
/// </summary>
public class BlockWriteStreamTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(1024)]
    public void Write_VaryingBuffer_WritesSameLength(int bufferLength)
    {
        // Arrange
        var source = new MemoryStream();
        var content = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        using var sut = new BlockWriteStream(source, bufferLength);

        // Act
        var written = sut.Write(content);

        // Assert
        written.Should().Be(content.Length);
    }
}
