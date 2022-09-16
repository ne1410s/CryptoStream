using Crypto.Encoding;
using Crypto.Hashing;
using Crypto.IO;
using Crypto.Streams;
using Crypto.Tests.TestObjects;
using Crypto.Transform;

namespace Crypto.Tests.Streams;

/// <summary>
/// Tests for the <see cref="CryptoBlockReadStream"/>.
/// </summary>
public class CryptoBlockReadStreamTests
{
    [Fact]
    public void Ctor_WithDecryptor_CallsReadPepper()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", "pixel.png"));
        fi.EncryptInSitu(TestRefs.TestKey);
        var mockDecryptor = new Mock<IGcmDecryptor>();

        // Act
        _ = new CryptoBlockReadStream(fi, TestRefs.TestKey, decryptor: mockDecryptor.Object);

        // Assert
        mockDecryptor.Verify(m => m.ReadPepper(It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public void Read_BadOffset_ReturnsExpected()
    {
        // Arrange
        const int bufferLength = 1024;
        var fi = new FileInfo(Path.Combine("TestFiles", "earth.webm"));
        fi.EncryptInSitu(TestRefs.TestKey, bufferLength: bufferLength);
        var sut = new CryptoBlockReadStream(fi, TestRefs.TestKey, bufferLength);
        sut.Seek(12);

        // Act
        var block = sut.Read();
        var blockHashHex = block.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        blockHashHex.Should().Be("5c078aa8fb0d4ea759fa2f91e36e48ad");
    }

    [Fact]
    public void Read_SpanTwoBlocks_DecryptsTwoBlocks()
    {
        // Arrange
        const int bufferLength = 1024;
        var fi = new FileInfo(Path.Combine("TestFiles", "tennis.png"));
        fi.EncryptInSitu(TestRefs.TestKey, bufferLength: bufferLength);
        var salt = fi.ToSalt();
        var mockDecryptor = new Mock<IGcmDecryptor>();
        using var stream = fi.OpenRead();
        var sut = new CryptoBlockReadStream(stream, salt, TestRefs.TestKey, bufferLength, mockDecryptor.Object);
        sut.Seek(12);

        // Act
        _ = sut.Read();

        // Assert
        mockDecryptor.Verify(
            m => m.DecryptBlock(It.IsAny<GcmEncryptedBlock>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), false),
            Times.Exactly(2));
    }

    [Fact]
    public void Props_WhenPopulated_ShouldBeExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", "earth.avi"));
        fi.EncryptInSitu(TestRefs.TestKey);
        var salt = fi.ToSalt();
        using var stream = fi.OpenRead();
        var sut = new CryptoBlockReadStream(stream, salt, TestRefs.TestKey);

        // Act
        var uri = sut.Uri;
        var length = sut.Length;

        // Assert
        uri.Should().NotBeEmpty();
        length.Should().Be(fi.Length - 32);
    }
}
