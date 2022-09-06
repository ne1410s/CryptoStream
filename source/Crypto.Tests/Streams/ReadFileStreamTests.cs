using Crypto.Streams;

namespace Crypto.Tests.Streams;

/// <summary>
/// Tests for the <see cref="ReadFileStream"/> class.
/// </summary>
public class ReadFileStreamTests
{
    [Fact]
    public void Flush_WhenCalled_ThrowsNotSupported()
    {
        // Arrange
        using var sut = new ReadFileStream(new MemoryStream());

        // Act
        var act = () => sut.Flush();

        // Assert
        act.Should().ThrowExactly<NotSupportedException>()
            .WithMessage("This stream is read-only.");
    }

    [Fact]
    public void SetLength_WhenCalled_ThrowsNotSupported()
    {
        // Arrange
        using var sut = new ReadFileStream(new MemoryStream());

        // Act
        var act = () => sut.SetLength(0);

        // Assert
        act.Should().ThrowExactly<NotSupportedException>()
            .WithMessage("This stream is read-only.");
    }

    [Fact]
    public void Write_WhenCalled_ThrowsNotSupported()
    {
        // Arrange
        using var sut = new ReadFileStream(new MemoryStream());

        // Act
        var act = () => sut.Write(new byte[] { 1 }, 0, 1);

        // Assert
        act.Should().ThrowExactly<NotSupportedException>()
            .WithMessage("This stream is read-only.");
    }
}
