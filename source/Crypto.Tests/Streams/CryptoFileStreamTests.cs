using Crypto.Streams;

namespace Crypto.Tests.Streams;

/// <summary>
/// Tests for the <see cref="CryptoFileStream"/> class.
/// </summary>
public class CryptoFileStreamTests
{
    [Fact]
    public void Ctor_GetLength_AccountsForPepperLength()
    {
        // Arrange
        const int pepperLength = 32;
        var bytes = Enumerable.Repeat((byte)1, 40).ToArray();
        var stream = new MemoryStream(bytes);

        // Act
        var result = new CryptoFileStream(stream, new byte[] { 1 }, new byte[] { 2 });

        // Assert
        result.Length.Should().Be(stream.Length - pepperLength);
    }
}
