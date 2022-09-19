using Crypto.Encoding;
using Crypto.Hashing;
using Crypto.Streams;
using Crypto.Utils;

namespace Crypto.Tests.Streams;

/// <summary>
/// Tests for the <see cref="BlockReadStream"/>.
/// </summary>
public class BlockReadStreamTests
{
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
        blockHashHex.Should().Be("d1d4dda61babeaa854aa2dacb236c32d");
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
