using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace Crypto.Tests;

/// <summary>
/// Tests for the <see cref="AssertExtensions"/> class.
/// </summary>
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
    public void AssertReadable_StreamIsNull_ThrowsException()
    {
        // Arrange
        var mock = (Stream?)null;

        // Act
        var act = () => mock.AssertReadable();

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Stream not readable");
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
    public void AssertReadable_CannotSeekAndNotAtStartWithCheckImposed_ThrowsException()
    {
        // Arrange
        using var mock = new UnseekableStream(20);
        mock.Read(new byte[1], 0, 1);

        // Act
        var act = () => mock.AssertReadable(true);

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Stream not readable");
    }

    [Fact]
    public void AssertReadable_CannotSeekAndNotAtStartWithNoCheck_DoesNotThrow()
    {
        // Arrange
        using var mock = new UnseekableStream(20);
        mock.Read(new byte[1], 0, 1);

        // Act
        var act = () => mock.AssertReadable(false);

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
    public void AssertWriteable_StreamIsNull_ThrowsException()
    {
        // Arrange
        var mock = (Stream?)null;

        // Act
        var act = () => mock.AssertWriteable();

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Stream not writeable");
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

    [Fact]
    public void AssertWriteable_CannotSeekAndNotAtStart_ThrowsException()
    {
        // Arrange
        using var mock = new UnseekableStream(20);
        mock.Read(new byte[1], 0, 1);

        // Act
        var act = () => mock.AssertWriteable();

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Stream not writeable");
    }

    [Fact]
    public void AssertReusable_CannotReuseTransform_ThrowsException()
    {
        // Arrange
        var mock = new UnreusableHash();

        // Act
        var act = () => mock.AssertReusable();

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Hash algorithm not reusable");
    }

    [Fact]
    public void AssertExists_DoesNotExist_ThrowsException()
    {
        // Arrange
        var mock = new FileInfo(Guid.NewGuid().ToString());

        // Act
        var act = () => mock.AssertExists();

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("File not found: *");
    }

    [Fact]
    public void AssertExists_Exists_DoesNotThrow()
    {
        // Arrange
        var di = new DirectoryInfo("./");
        var mock = di.EnumerateFiles().First();

        // Act
        var act = () => mock.AssertExists();

        // Assert
        act.Should().NotThrow();
    }

    private class UnreusableHash : HMACMD5
    {
        public override bool CanReuseTransform => false;
    }

    private class UnseekableStream : MemoryStream
    {
        public UnseekableStream(int length = 0)
            : base(Enumerable.Range(0, length).Select(_ => (byte)1).ToArray())
        { }

        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin loc)
            => throw new NotSupportedException();
    }
}