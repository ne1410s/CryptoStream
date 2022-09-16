using Crypto.Encoding;
using Crypto.Hashing;
using Crypto.Streams;
using Crypto.Tests.TestObjects;

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
        var fi = new FileInfo(Path.Combine("TestFiles", "earth2.webm"));
        var sut = new BlockReadStream(fi, bufferLength);
        sut.Position = 12;

        // Act
        var block = sut.Read();
        var blockHashHex = block.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        blockHashHex.Should().Be("5c078aa8fb0d4ea759fa2f91e36e48ad");
    }

    [Fact]
    public void Read_NearEnd_ResizesBuffer()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", "earth2.webm"));
        var bufferLength = fi.Length + 1024;
        var sut = new BlockReadStream(fi, (int)bufferLength);
        sut.Seek(fi.Length - 900);

        // Act
        var block = sut.Read();

        // Assert
        block.Length.Should().Be((int)fi.Length);
    }
}
