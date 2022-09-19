using Crypto.IO;
using Crypto.Tests.TestObjects;
using Crypto.Transform;
using Crypto.Utils;

namespace Crypto.Tests.Transform;

/// <summary>
/// Tests for the <see cref="AesGcmDecryptor"/>.
/// </summary>
public class AesGcmDecryptorTests
{
    [Fact]
    public void Decrypt_OversizedTarget_CallsResize()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!");
        fi.EncryptInSitu(TestRefs.TestKey);
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmDecryptor(resizer: mockResizer.Object);
        var salt = fi.ToSalt();
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt);

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, 4),
            Times.Once);
    }

    [Fact]
    public void Decrypt_OversizedTarget_GetsResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hey!");
        fi.EncryptInSitu(TestRefs.TestKey);
        using var trgStream = new MemoryStream(Enumerable.Repeat((byte)1, 20).ToArray());

        // Act
        fi.DecryptTo(trgStream, TestRefs.TestKey);

        // Assert
        trgStream.Length.Should().Be(4);
    }

    [Fact]
    public void Decrypt_EqualSizedTarget_NotResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!");
        const int bufferLength = 4;
        fi.EncryptInSitu(TestRefs.TestKey, bufferLength: bufferLength);
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmDecryptor(resizer: mockResizer.Object);
        var salt = fi.ToSalt();
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt, bufferLength);

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, It.IsAny<int>()),
            Times.Never);
    }
}
