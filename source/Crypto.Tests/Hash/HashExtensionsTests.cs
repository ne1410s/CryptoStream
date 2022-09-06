using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Codec;
using Crypto.Hash;

namespace Crypto.Tests.Hash;

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
        var algo = (HashAlgo)999;

        // Act
        var act = () => new byte[] { 1 }.Hash(algo);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*unsupported*");
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
}
