using Crypto.Codec;

namespace Crypto.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void DeriveKey_VaryingSourceOrder_SameResult()
    {
        // Arrange
        const string seed = "1234";
        var sources = new byte[][]
        {
            new byte[] { 1 },
            new byte[] { 2 },
            new byte[] { 3 },
        };

        // Act
        var result1 = seed.DeriveKey(sources).AsString(ByteCodec.Hex);
        var result2 = seed.DeriveKey(sources.Reverse().ToArray()).AsString(ByteCodec.Hex);

        // Assert
        result1.Should().Be(result2).And.Be("67e85f44ef4c87d071e9c5f5b71f6bc1e963b233");
    }
}
