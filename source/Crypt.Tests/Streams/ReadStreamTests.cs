using Crypt.Streams;

namespace Crypt.Tests.Streams;

/// <summary>
/// Tests for the <see cref="ReadStream"/>
/// </summary>
public class ReadStreamTests
{
    [Fact]
    public void ReadStream_PropertyCheck_IsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var sut = new ReadStream(fi);
        var buffer = new byte[32];

        // Act
        sut.Read(buffer, 0, buffer.Length);

        // Assert
        sut.CanRead.Should().BeTrue();
        sut.CanSeek.Should().BeTrue();
        sut.CanWrite.Should().BeFalse();
        sut.Length.Should().Be(fi.Length);
    }

    [Fact]
    public void Flush_WhenCalled_ThrowsException()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var sut = new ReadStream(fi);

        // Act
        var act = () => sut.Flush();

        // Assert
        act.Should().ThrowExactly<NotSupportedException>()
            .WithMessage("This stream is read-only.");
    }

    [Fact]
    public void SetLength_WhenCalled_ThrowsException()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var sut = new ReadStream(fi);

        // Act
        var act = () => sut.SetLength(fi.Length);

        // Assert
        act.Should().ThrowExactly<NotSupportedException>()
            .WithMessage("This stream is read-only.");
    }

    [Fact]
    public void Write_WhenCalled_ThrowsException()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var sut = new ReadStream(fi);
        var buffer = new byte[] { 2 };

        // Act
        var act = () => sut.Write(buffer, 0, buffer.Length);

        // Assert
        act.Should().ThrowExactly<NotSupportedException>()
            .WithMessage("This stream is read-only.");
    }

    [Fact]
    public void Dispose_WithStream_IsDisposed()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        var sut = new ReadStream(fi);

        // Act
        sut.Dispose();
        var act = () => sut.Length;

        // Assert
        act.Should().ThrowExactly<ObjectDisposedException>()
            .WithMessage("Cannot access a closed file.");
    }
}
