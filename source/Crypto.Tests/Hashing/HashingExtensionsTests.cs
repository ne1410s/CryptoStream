using Crypto.Encoding;
using Crypto.Hashing;

namespace Crypto.Tests.Hashing;

/// <summary>
/// Tests for the <see cref="HashingExtensions"/> class.
/// </summary>
public class HashingExtensionsTests
{
    [Theory]
    [InlineData(HashType.Md5, "Uonfc331cyb83SJZevsfrA==")]
    [InlineData(HashType.Sha1, "cDeAcZjCKn0rCAc3HXY3eahP388=")]
    [InlineData(HashType.Sha256, "A5BYxvLAy0ksUzsKTRTvd8wPeKvMztUofYShogEc+4E=")]
    [InlineData(HashType.Sha384, "hiKdxtL/vqxzgHRBVKpwApHAZDUqDb3He57T8sjh2sTcMlhn053f8dJim3o5PUf2")]
    [InlineData(HashType.Sha512, "J4ZMxSGalRp6blK4yN3faYHQmNoWWNliWMhwssiN+8tRhBrqFyoouvpqeXMRZVhGdwZgRclZ7Q+ZKWiNBN78KQ==")]
    public void Hash_VaryingHashType_ReturnsExpected(HashType mode, string expectedBase64)
    {
        // Arrange
        var testArray = new byte[] { 1, 2, 3 };

        // Act
        var hashBase64 = testArray.Hash(mode).Encode(Codec.ByteBase64);

        // Assert
        hashBase64.Should().Be(expectedBase64);
    }

    [Fact]
    public void HashBadHashType_ThrowsArgumentException()
    {
        // Arrange
        var testArray = new byte[] { 1, 2, 3 };
        const HashType badType = (HashType)9999;

        // Act
        var act = () => testArray.Hash(badType).Encode(Codec.ByteBase64);

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Bad hash mode: 9999 (Parameter 'mode')");
    }
}
