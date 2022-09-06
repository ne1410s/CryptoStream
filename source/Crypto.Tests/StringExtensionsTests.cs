namespace Crypto.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void DeriveKey_WithParams_ReturnsExpected()
    {
        // Arrange
        const string seed = "1234";
        var sources = new byte[][] { new byte[] { 1 } };
        var expected = new byte[] { 51, 30, 208, 201, 95, 194, 53, 180, 106, 245, 100, 182, 196, 217, 173, 198, 0, 57, 239, 34 };

        // Act
        var result = seed.DeriveKey(sources);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

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
        var result1 = seed.DeriveKey(sources);
        var result2 = seed.DeriveKey(sources.Reverse().ToArray());

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }
}
