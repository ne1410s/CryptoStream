﻿using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.Streams;
using Crypt.Utils;

namespace Crypt.Tests.Streams;

/// <summary>
/// Tests for the <see cref="BlockReadStream"/>.
/// </summary>
public class BlockReadStreamTests
{
    [Fact]
    public void Ctor_SpecificMedia_HashExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.avi"));
        var sut = new BlockReadStream(fi);
        const string expectedMd5Hex = "91d326694fdff83d0df74c357f3feb84";

        // Act
        var actualMd5Hex = sut.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        actualMd5Hex.Should().Be(expectedMd5Hex);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(327680)]
    [InlineData(3370848)]
    [InlineData(3384800)]
    public void Read_VaryingStartPosition_MimicsNonBlockingAuthority(long position, int bufferLength = 32768)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("Samples", "sample.avi"));
        using var authority = new SimpleFileStream(fi, bufferLength);
        authority.Seek(position, SeekOrigin.Begin);
        var authBuffer = new byte[bufferLength];
        var authRead = authority.Read(authBuffer, 0, bufferLength);
        if (authRead < bufferLength) { Array.Resize(ref authBuffer, authRead); }
        var authMd5Hex = authBuffer.Hash(HashType.Md5).Encode(Codec.ByteHex);
        using var sut = new BlockReadStream(fi);
        sut.Seek(position, SeekOrigin.Begin);

        // Act
        var result = sut.Read();
        var resultMd5Hex = result.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        resultMd5Hex.Should().Be(authMd5Hex);
        sut.Position.Should().Be(authority.Position);
    }

    [Fact]
    public void Read_BadOffset_ReturnsExpected()
    {
        // Arrange
        const int bufferLength = 1024;
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "this is a string that is for sure more than twelve bytes!");
        var sut = new BlockReadStream(fi, bufferLength);
        sut.Seek(12);

        // Act
        var block = sut.Read();
        var blockHashHex = block.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        blockHashHex.Should().Be("2645d0e13dc5bf622a89da2b76fb5cf5");
    }

    [Fact]
    public void Read_BlocksRemain_SetToExpectedPosition()
    {
        // Arrange
        const int bufferLength = 8;
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "this is a string that is for sure more than twelve bytes!");
        var sut = new BlockReadStream(fi, bufferLength);
        sut.Seek(6);

        // Act
        _ = sut.Read();

        // Assert
        sut.Position.Should().Be(14);
    }

    [Fact]
    public void Read_OversizedBuffer_CallsResize()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "this is a string that is of some size.");
        const int bufferLength = 1024;
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new BlockReadStream(fi, bufferLength, mockResizer.Object);
        sut.Seek(fi.Length - 9);

        // Act
        var block = sut.Read();

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, 38),
            Times.Once);
    }

    [Fact]
    public void Read_OversizedBuffer_ResizesBuffer()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "this is a string that is of some size.");
        const int bufferLength = 1024;
        var sut = new BlockReadStream(fi, bufferLength);
        sut.Seek(fi.Length - 9);

        // Act
        var block = sut.Read();

        // Assert
        block.Length.Should().Be((int)fi.Length);
    }

    [Fact]
    public void Read_PerfectFitBuffer_NotResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello here is a string");
        var bufferLength = fi.Length;
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new BlockReadStream(fi, (int)bufferLength, mockResizer.Object);
        sut.Seek(12);

        // Act
        _ = sut.Read();

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public void Read_BytesWrittenEqualsCount_TerminatesLoop()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello here is a string");
        var sut = new BlockReadStream(fi);
        var buffer = new byte[fi.Length];

        // Act
        var read = sut.Read(buffer, 0, buffer.Length - 1);

        // Assert
        read.Should().Be(buffer.Length - 1);
    }

    [Fact]
    public void Read_BytesWrittenPlusOffsetEqualsLength_TerminatesLoop()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello here is a string");
        var sut = new BlockReadStream(fi);
        var buffer = new byte[fi.Length];

        // Act
        var read = sut.Read(buffer, 20, buffer.Length - 1);

        // Assert
        read.Should().Be(2);
    }
}
