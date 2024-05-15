// <copyright file="BlockStreamUtilsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Utils;

/// <summary>
/// Tests for the <see cref="StreamBlockUtils"/>.
/// </summary>
public class BlockStreamUtilsTests
{
    [Fact]
    public void BlockPosition_Midway_ReturnsExpected()
    {
        // Arrange
        const long initial = 12;
        const int buffer = 10;

        // Act
        var result = StreamBlockUtils.BlockPosition(initial, buffer, out var blockNo, out var remains);

        // Assert
        result.Should().Be(10);
        blockNo.Should().Be(2);
        remains.Should().Be(2);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(12, 0, 20)]
    [InlineData(499, 0, 500)]
    [InlineData(499, 2, 600)]
    [InlineData(500, 0, 500)]
    [InlineData(10_023_304_423, 0, 10_024_000_000)]
    public void GetPadSize_VaryingInputs_ReturnsExpected(long length, long reserve, long expected)
    {
        // Arrange & Act
        var result = StreamBlockUtils.GetPadSize(length, reserve);

        // Assert
        result.Should().Be(expected);
    }
}
