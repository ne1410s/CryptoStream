// <copyright file="BlockStreamUtilsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Tests.Streams;

using Crypt.Utils;

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
}
