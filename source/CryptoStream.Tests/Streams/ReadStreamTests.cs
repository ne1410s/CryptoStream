// <copyright file="ReadStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Streams;

/// <summary>
/// Tests for the <see cref="SimpleStream"/>.
/// </summary>
public class ReadStreamTests
{
    [Fact]
    public void ReadStream_PropertyCheck_IsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var sut = fi.OpenSimpleRead();
        var buffer = new byte[32];

        // Act
        sut.Read(buffer, 0, buffer.Length);

        // Assert
        sut.CanRead.Should().BeTrue();
        sut.CanSeek.Should().BeTrue();
        sut.CanWrite.Should().BeFalse();
        sut.Length.Should().Be(fi.Length);
    }

    [Fact]
    public void Ctor_NullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => fi.OpenSimpleRead();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Flush_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var sut = fi.OpenSimpleRead();

        // Act
        var act = sut.Flush;

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SetLength_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var sut = fi.OpenSimpleWrite();

        // Act
        var act = () => sut.SetLength(fi.Length);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Write_WhenCalled_DoesNotThrow()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var sut = fi.OpenSimpleWrite();
        var buffer = new byte[] { 2 };

        // Act
        var act = () => sut.Write(buffer, 0, buffer.Length);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_WithStream_IsDisposed()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        var sut = fi.OpenSimpleRead();

        // Act
        sut.Dispose();
        var act = () => sut.Length;

        // Assert
        act.Should().ThrowExactly<ObjectDisposedException>()
            .WithMessage("Cannot access a closed file.");
    }
}
