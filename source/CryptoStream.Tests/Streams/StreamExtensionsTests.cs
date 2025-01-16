// <copyright file="StreamExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Streams;
using CryptoStream.Tests.TestObjects;

/// <summary>
/// Tests for the <see cref="StreamExtensions"/> class.
/// </summary>
public class StreamExtensionsTests
{
    [Fact]
    public void Reset_NullStream_ThrowsException()
    {
        // Arrange
        var stream = (Stream)null!;

        // Act
        var act = () => stream.Reset();

        // Assert
        _ = act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Reset_UnseekableButAtStart_DoesNotThrow()
    {
        // Arrange
        using var stream = new UnseekableStream([1, 2, 3]);

        // Act
        var act = () => stream.Reset();

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void Reset_UnseekableButNotAtStart_ThrowsException()
    {
        // Arrange
        using var stream = new UnseekableStream([1, 2, 3]);
        _ = stream.ReadByte();

        // Act
        var act = () => stream.Reset();

        // Assert
        _ = act.ShouldThrow<NotSupportedException>();
    }

    [Fact]
    public void Reset_UnseekableNotAtStartButSkipped_DoesNotThrow()
    {
        // Arrange
        using var stream = new UnseekableStream([1, 2, 3]);
        _ = stream.ReadByte();

        // Act
        var act = () => stream.Reset(true);

        // Assert
        act.ShouldNotThrow();
    }
}
