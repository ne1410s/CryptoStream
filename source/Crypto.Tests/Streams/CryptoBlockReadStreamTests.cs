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
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello here is another string");
        fi.EncryptInSitu(TestRefs.TestKey);
        var mockDecryptor = new Mock<IGcmDecryptor>();

        // Act
        _ = new CryptoBlockReadStream(fi, TestRefs.TestKey, decryptor: mockDecryptor.Object);

        // Assert
        mockDecryptor.Verify(m => m.ReadPepper(It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public void Read_SpanTwoBlocks_DecryptsTwoBlocks()
    {
        // Arrange
        const int bufferLength = 16;
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "this is a sentence more than twelve bytes for sure!");
        fi.EncryptInSitu(TestRefs.TestKey, bufferLength: bufferLength);
        var salt = fi.ToSalt();
        var mockDecryptor = new Mock<IGcmDecryptor>();
        using var stream = fi.OpenRead();
        var sut = new CryptoBlockReadStream(stream, salt, TestRefs.TestKey, bufferLength, mockDecryptor.Object);
        sut.Seek(12);

        // Act
        var block = sut.Read();

        // Assert
        mockDecryptor.Verify(
            m => m.DecryptBlock(It.IsAny<GcmEncryptedBlock>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), false),
            Times.Exactly(2));
    }

    [Fact]
    public void Props_WhenPopulated_ShouldBeExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
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
