using Crypto.Encoding;
using Crypto.Tests.TestObjects;
using Crypto.Transform;
using Crypto.Utils;

namespace Crypto.Tests.Transform;

/// <summary>
/// Test for the <see cref="AesGcmEncryptor"/>.
/// </summary>
public class AesGcmEncryptorTests
{
    [Fact]
    public void GenerateSalt_WithStream_ReturnsExpected()
    {
        // Arrange
        var sut = new AesGcmEncryptor();
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var salt = sut.GenerateSalt(stream);

        // Assert
        salt.Encode(Codec.ByteHex).Should().Be("5890032c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81c6f2c0cb49");
    }

    [Fact]
    public void Encrypt_OversizedTarget_GetsResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!!");
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmEncryptor(resizer: mockResizer.Object);
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        sut.Encrypt(srcStream, trgStream, TestRefs.TestKey);

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, 5),
            Times.Once);
    }

    [Fact]
    public void Encrypt_EqualSizedTarget_NotResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!!");
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmEncryptor(resizer: mockResizer.Object);
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, bufferLength: 5);

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, It.IsAny<int>()),
            Times.Never);
    }
}
