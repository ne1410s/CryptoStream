namespace Crypto.Tests;

public class AssertExtensionsTests
{
    [Fact]
    public void AssertReadable_IsReadable_DoesNotThrow()
    {
        // Arrange
        using var mock = new MemoryStream(new byte[] { 1, 2 });

        // Act
        var act = () => mock.AssertReadable();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AssertReadable_ZeroLength_ThrowsException()
    {
        // Arrange
        using var mock = new MemoryStream(Array.Empty<byte>());

        // Act
        var act = () => mock.AssertReadable();

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Stream not readable");
    }

    [Fact]
    public void AssertWriteable_IsWriteable_DoesNotThrow()
    {
        // Arrange
        using var mock = new MemoryStream(new byte[] { 1, 2 });

        // Act
        var act = () => mock.AssertWriteable();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AssertWriteable_ZeroLengthButWriteable_DoesNotThrow()
    {
        // Arrange
        using var mock = new MemoryStream(Array.Empty<byte>());

        // Act
        var act = () => mock.AssertWriteable();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AssertWriteable_NotWriteable_ThrowsException()
    {
        // Arrange
        using var mock = new MemoryStream(new byte[] { 1, 2 }, false);

        // Act
        var act = () => mock.AssertWriteable();

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Stream not writeable");
    }
}