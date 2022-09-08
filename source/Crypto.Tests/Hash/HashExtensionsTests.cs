using Crypto.Codec;
using Crypto.Hash;
using Crypto.Tests.TestHelpers;

namespace Crypto.Tests.Hash;

/// <summary>
/// Tests for the <see cref="HashExtensions"/> class.
/// </summary>
public class HashExtensionsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(8)]
    public void Hash_VaryOriginalPosition_DoesNotChangeResult(int seekTo)
    {
        // Arrange
        const string expected = "8596c1af55b14b7b320112944fcb8536";
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        stream.Seek(seekTo, SeekOrigin.Begin);

        // Act
        var result = stream.Hash(HashAlgo.Md5).AsString(ByteCodec.Hex);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Hash_FromFile_ReturnsExpected()
    {
        // Arrange
        const string expected = "WyDwjZRpi3/YA0tJ7JyXDs5l3AWs46R5lcA31SGYOHyuq5yckdpti87QRH33CKCIzZbxDcUl2SeOdNNPFn7CmQ==";
        var fi = new FileInfo(Path.Combine("img", "test.jpg"));

        // Act
        var result = fi.Hash(HashAlgo.Sha512).AsString(ByteCodec.Base64);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Hash_BadAlgo_ThrowsException()
    {
        // Arrange
        const HashAlgo algo = (HashAlgo)999;

        // Act
        var act = () => new byte[] { 1 }.Hash(algo);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*unsupported*");
    }

    [Fact]
    public void Hash_UnreadableStream_ThrowsException()
    {
        // Arrange
        var stream = new UnreadableStream();

        // Act
        var act = () => stream.Hash(HashAlgo.Md5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Stream not readable");
    }

    [Fact]
    public void Hash_File_ReturnsExpected()
    {
        // Arrange
        const string fileName = nameof(Hash_File_ReturnsExpected);
        File.WriteAllText(fileName, "hello world");

        // Act
        var result = new FileInfo(fileName).Hash(HashAlgo.Md5).AsString(ByteCodec.Hex);
        File.Delete(fileName);

        // Assert
        result.Should().Be("5eb63bbbe01eeed093cb22bb8f5acdc3");
    }

    [Fact]
    public void LightHash_FromFile_ReturnsExpected()
    {
        // Arrange
        const string expected = "XD5Rz7ax8Vvt5eyViNkSYEbFnZ2kHyatHlBWhLSMsecQMbQVHgNOvSDAXbEiqokP";
        var fi = new FileInfo(Path.Combine("img", "test.jpg"));

        // Act
        var result = fi.LightHash(HashAlgo.Sha384).AsString(ByteCodec.Base64);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void LightHash_FromStreamVariousPositions_ReturnsSame(int position)
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        stream.Seek(position, SeekOrigin.Begin);

        // Act
        var result = stream.LightHash(HashAlgo.Sha1).AsString(ByteCodec.Base64);

        // Assert
        result.Should().Be("IKSTAuZ5pROok6Vh0EZCYi4aFOs=");
    }

    [Fact]
    public void LightHash_ValidInput_SetsPositionToStart()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        stream.Seek(3, SeekOrigin.Begin);

        // Act
        stream.LightHash(HashAlgo.Sha1).AsString(ByteCodec.Base64);

        // Assert
        stream.Position.Should().Be(0);
    }

    [Fact]
    public void LightHash_UnreadableStream_ThrowsException()
    {
        // Arrange
        using var stream = new UnreadableStream();

        // Act
        var act = () => stream.LightHash(HashAlgo.Sha1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Stream not readable");
    }

    [Fact]
    public void LightHash_UnseekableStream_ThrowsException()
    {
        // Arrange
        using var stream = new UnseekableStream(5);

        // Act
        var act = () => stream.LightHash(HashAlgo.Sha1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Stream not readable");
    }
}
