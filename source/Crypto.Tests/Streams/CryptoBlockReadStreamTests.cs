using Crypto.Encoding;
using Crypto.Hashing;
using Crypto.IO;
using Crypto.Streams;
using Crypto.Tests.TestObjects;

namespace Crypto.Tests.Streams;

/// <summary>
/// Tests for the <see cref="CryptoBlockReadStream"/>.
/// </summary>
public class CryptoBlockReadStreamTests
{
    [Fact]
    public void Read_BadOffset_ReturnsExpected()
    {
        // Arrange
        const int bufferLength = 1024;
        var fi = new FileInfo(Path.Combine("TestFiles", "earth.webm"));
        fi.EncryptInSitu(TestRefs.TestKey, bufferLength: bufferLength);
        var sut = new CryptoBlockReadStream(fi, TestRefs.TestKey, bufferLength);
        sut.Position = 12;

        // Act
        var block = sut.Read();
        var blockHashHex = block.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        blockHashHex.Should().Be("5c078aa8fb0d4ea759fa2f91e36e48ad");
    }

    [Fact]
    public void Uri_WhenPopulated_IsNotEmpty()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", "earth.avi"));
        fi.EncryptInSitu(TestRefs.TestKey);
        var sut = new CryptoBlockReadStream(fi, TestRefs.TestKey);

        // Act
        var uri = sut.Uri;

        // Assert
        uri.Should().NotBeEmpty();
    }
}
