using Crypto.Streams;

namespace Crypto.Tests.Streams;

public class ChunkingFileStreamTests
{
    [Fact]
    public void Ctor_WithSameFile_GeneratesDifferentPseudoUris()
    {
        // Arrange
        var fi = new DirectoryInfo(".").EnumerateFiles().First();

        // Act
        using var sut1 = new ChunkingFileStream(fi);
        using var sut2 = new ChunkingFileStream(fi);

        // Assert
        sut1.Uri.Should().NotBe(sut2.Uri).And.NotBeEmpty();
    }

    [Fact]
    public void Ctor_WithStream_GeneratesPseudoUri()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        using var sut = new ChunkingFileStream(stream);

        // Assert
        sut.Uri.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(5555555)]
    public void Ctor_WithChunkSize_GivenBufferLength(int chunkSize)
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        using var sut = new ChunkingFileStream(stream, chunkSize);

        // Assert
        sut.BufferLength.Should().Be(chunkSize);
    }

    [Fact]
    public void Seek_NotFromStart_SeeksAbsoluteAmountFromStart()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        stream.Position = 3;
        using var sut = new ChunkingFileStream(stream, 2);

        // Act
        var result = sut.Seek(3);

        // Assert
        result.Should().Be(3).And.Be(sut.Position);
    }

    [Fact]
    public void Read_NotFromStart_ReadsChunk()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        stream.Position = 3;
        using var sut = new ChunkingFileStream(stream, 2);

        // Act
        var result = sut.Read();

        // Assert
        result.Should().BeEquivalentTo(new byte[] { 4, 5 });
    }

    [Fact]
    public void Read_FinalChunkExceedingStreamLength_ProducesResizedArray()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        stream.Position = 6;
        using var sut = new ChunkingFileStream(stream, 6);

        // Act
        var result = sut.Read();

        // Assert
        result.Should().BeEquivalentTo(new byte[] { 7, 8 });
    }
}
