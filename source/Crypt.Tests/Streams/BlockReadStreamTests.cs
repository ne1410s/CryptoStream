// <copyright file="BlockReadStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Tests.Streams;

using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.Streams;
using Crypt.Utils;

/// <summary>
/// Tests for the <see cref="BlockReadStream"/>.
/// </summary>
public class BlockReadStreamTests
{
    [Fact]
    public void Ctor_SpecificMedia_HashExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", "sample.avi"));
        var sut = new BlockReadStream(fi);
        const string expectedMd5Hex = "91d326694fdff83d0df74c357f3feb84";

        // Act
        var actualMd5Hex = sut.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        actualMd5Hex.Should().Be(expectedMd5Hex);
    }

    [Fact]
    public void Ctor_NullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => new BlockReadStream(fi);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(327680)]
    [InlineData(3370848)]
    [InlineData(3384800)]
    public void Read_VaryingStartPosition_MimicsNonBlockingAuthority(long position, int bufferLength = 32768)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", "sample.avi"));
        using var authority = new SimpleFileStream(fi, bufferLength);
        authority.Seek(position, SeekOrigin.Begin);
        var authBuffer = new byte[bufferLength];
        var authRead = authority.Read(authBuffer, 0, bufferLength);
        if (authRead < bufferLength)
        {
            Array.Resize(ref authBuffer, authRead);
        }

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
        using var sut = new BlockReadStream(fi, bufferLength);
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
        using var sut = new BlockReadStream(fi, bufferLength, mockResizer.Object);
        sut.Seek(fi.Length - 9);

        // Act
        _ = sut.Read();

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, 9),
            Times.Once);
    }

    [Fact]
    public void Read_OversizedBuffer_ResizesBuffer()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "this is a string that is of some size.");
        const int bufferLength = 1024;
        using var sut = new BlockReadStream(fi, bufferLength);
        sut.Seek(fi.Length - 9);

        // Act
        var block = sut.Read();

        // Assert
        block.Length.Should().Be(9);
    }

    [Fact]
    public void Read_PerfectFitBuffer_NotResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello here is a string");
        var bufferLength = fi.Length;
        var mockResizer = new Mock<IArrayResizer>();
        using var sut = new BlockReadStream(fi, (int)bufferLength, mockResizer.Object);

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
        using var sut = new BlockReadStream(fi);
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
        using var sut = new BlockReadStream(fi);
        var buffer = new byte[fi.Length];

        // Act
        var read = sut.Read(buffer, 20, buffer.Length - 1);

        // Assert
        read.Should().Be(21);
    }

    [Fact]
    public void SubclassMapBlock_WithBuffer_DoesNotThrow()
    {
        // Arrange
        var sut = new VanillaBlockReadStream(new MemoryStream());

        // Act
        var act = () => sut.TestMapBlock(Array.Empty<byte>());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SubclassMapBlock_NullBuffer_ThrowsException()
    {
        // Arrange
        var sut = new VanillaBlockReadStream(new MemoryStream());

        // Act
        var act = () => sut.TestMapBlock(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private class VanillaBlockReadStream : BlockReadStream
    {
        public VanillaBlockReadStream(Stream stream)
            : base(stream)
        { }

        public void TestMapBlock(byte[] sourceBuffer)
        {
            this.MapBlock(sourceBuffer, 1);
        }
    }
}
