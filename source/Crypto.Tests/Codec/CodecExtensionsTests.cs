using Crypto.Codec;

namespace Crypto.Tests.Codec;

/// <summary>
/// Tests for the <see cref="CodecExtensions"/> class.
/// </summary>
public class CodecExtensionsTests
{
    [Theory]
    [InlineData(CharCodec.Ascii, new byte[] { 104, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100 })]
    [InlineData(CharCodec.Utf8, new byte[] { 104, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100 })]
    [InlineData(CharCodec.Unicode, new byte[] { 104, 0, 101, 0, 108, 0, 108, 0, 111, 0, 32, 0, 119, 0, 111, 0, 114, 0, 108, 0, 100, 0 })]
    public void AsString_VaryingCharCodec_ReturnsExpected(CharCodec codec, byte[] bytes)
    {
        // Arrange & Act
        var result = bytes.AsString(codec);

        // Assert
        result.Should().Be("hello world");
    }

    [Fact]
    public void AsString_BadCharCodec_ThrowsException()
    {
        // Arrange
        const CharCodec codec = (CharCodec)999;

        // Act
        var act = () => new byte[1].AsString(codec);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("* unsupported");
    }

    [Fact]
    public void AsString_BadByteCodec_ThrowsException()
    {
        // Arrange
        const ByteCodec codec = (ByteCodec)999;

        // Act
        var act = () => new byte[1].AsString(codec);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("* -> string unsupported");
    }

    [Theory]
    [InlineData(ByteCodec.Base64, new byte[] { 105, 183, 29 })]
    [InlineData(ByteCodec.Hex, new byte[] { 171, 205 })]
    public void AsBytes_VaryingByteCodec_ReturnsExpected(ByteCodec codec, byte[] expected)
    {
        // Arrange & Act
        var result = "abcd".AsBytes(codec);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void AsBytes_BadByteCodec_ThrowsException()
    {
        // Arrange
        const ByteCodec codec = (ByteCodec)999;

        // Act
        var act = () => "abcd".AsBytes(codec);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("* -> bytes unsupported");
    }
}
