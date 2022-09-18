using Crypto.IO;
using Crypto.Tests.TestObjects;
using Crypto.Transform;

namespace Crypto.Tests.Transform;

/// <summary>
/// Tests for the <see cref="AesGcmDecryptor"/>.
/// </summary>
public class AesGcmDecryptorTests
{
    [Fact]
    public void Decrypt_OversizedTarget_GetsResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!");
        fi.EncryptInSitu(TestRefs.TestKey);
        using var trgStream = new MemoryStream(Enumerable.Repeat((byte)1, 20).ToArray());

        // Act
        fi.DecryptTo(trgStream, TestRefs.TestKey);

        // Assert
        trgStream.Length.Should().Be(4);
    }
}
