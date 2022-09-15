using Crypto.Keying;

namespace Crypto.Tests.Keying;

/// <summary>
/// Tests for the <see cref="DefaultKeyDeriver"/>. class.
/// </summary>
public class DefaultKeyDeriverTests
{
    [Fact]
    public void DeriveKey_DifferentOrderHashes_ProduceSameResult()
    {
        // Arrange
        const string seed = "x";
        var sut = new DefaultKeyDeriver();
        var hash1 = new byte[] { 1, 2, 3 };
        var hash2 = new byte[] { 4, 5, 6 };

        // Act
        var key1 = sut.DeriveKey(seed, hash1, hash2);
        var key2 = sut.DeriveKey(seed, hash2, hash1);

        // Assert
        key1.Should().BeEquivalentTo(key2);
    }
}
