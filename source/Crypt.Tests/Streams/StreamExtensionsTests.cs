// <copyright file="StreamExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Tests.Streams;

using Crypt.Streams;
using Crypt.Tests.TestObjects;

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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reset_UnseekableButAtStart_DoesNotThrow()
    {
        // Arrange
        var stream = new UnseekableStream(new byte[] { 1, 2, 3 });

        // Act
        var act = () => stream.Reset();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Reset_UnseekableButNotAtStart_ThrowsException()
    {
        // Arrange
        var stream = new UnseekableStream(new byte[] { 1, 2, 3 });
        stream.ReadByte();

        // Act
        var act = () => stream.Reset();

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Reset_UnseekableNotAtStartButSkipped_DoesNotThrow()
    {
        // Arrange
        var stream = new UnseekableStream(new byte[] { 1, 2, 3 });
        stream.ReadByte();

        // Act
        var act = () => stream.Reset(true);

        // Assert
        act.Should().NotThrow();
    }
}
