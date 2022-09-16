using Crypto.Encoding;
using Crypto.Transform;

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
}
