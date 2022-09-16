using Crypto.Encoding;
using Crypto.Hashing;
using Crypto.IO;
using Crypto.Tests.TestObjects;
using Crypto.Transform;

namespace Crypto.Tests.IO;

/// <summary>
/// Tests for <see cref="FileExtensions"/>
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void DecryptTo_WithDecryptor_CallsDecrypt()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{TestRefs.CryptoFileName}.{Guid.NewGuid()}"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        var mockDecryptor = new Mock<IDecryptor>();
        var trgStream = new MemoryStream();

        // Act
        fi.DecryptTo(trgStream, TestRefs.TestKey, mockDecryptor.Object);

        // Assert
        mockDecryptor.Verify(m => m.Decrypt(It.IsAny<Stream>(), trgStream, TestRefs.TestKey, It.IsAny<byte[]>(), 32768, null));
    }

    [Fact]
    public void DecryptTo_DefaultDecryptor_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        var content = $"hi{Guid.NewGuid()}";
        File.WriteAllText(fi.FullName, content);
        fi.EncryptInSitu(TestRefs.TestKey);
        var trgStream = new MemoryStream();

        // Act
        fi.DecryptTo(trgStream, TestRefs.TestKey);

        // Assert
        trgStream.ToArray().Encode(Codec.CharUtf8).Should().Be(content);
    }

    [Fact]
    public void DecryptTo_WithMac_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        var content = $"hi{Guid.NewGuid()}";
        File.WriteAllText(fi.FullName, content);
        using var macStream = new MemoryStream();
        fi.EncryptInSitu(TestRefs.TestKey, mac: macStream);
        var trgStream = new MemoryStream();

        // Act
        fi.DecryptTo(trgStream, TestRefs.TestKey, mac: macStream);

        // Assert
        trgStream.ToArray().Encode(Codec.CharUtf8).Should().Be(content);
    }

    [Fact]
    public void EncryptInSitu_WithFile_UpdatesFileInfoReference()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");

        // Act
        var salt = fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        fi.Name.Should().Be(salt + ".txt");
        fi.Exists.Should().BeTrue();
    }

    [Fact]
    public void EncryptInSitu_WithMac_PopulatesMacStream()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var macStream = new MemoryStream();

        // Act
        var salt = fi.EncryptInSitu(TestRefs.TestKey, mac: macStream);

        // Assert
        macStream.ToArray().Length.Should().Be(16);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("T123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")]
    [InlineData(TestRefs.CryptoFileName + ".")]
    [InlineData(TestRefs.CryptoFileName + "0")]
    public void ToSalt_BadName_ThrowsArgumentException(string notASalt)
    {
        // Arrange
        var fi = new FileInfo(notASalt);

        // Act
        var act = () => fi.ToSalt();

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage($"Unable to obtain salt: '{notASalt}' (Parameter 'fileName')");
    }

    [Theory]
    [InlineData(TestRefs.CryptoFileName)]
    [InlineData(TestRefs.CryptoFileName + ".txt")]
    [InlineData(TestRefs.CryptoFileName + ".super_Ext-12")]

    public void ToSalt_GoodName_ReturnsValue(string notASalt)
    {
        // Arrange
        var fi = new FileInfo(notASalt);

        // Act
        var salt = fi.ToSalt();

        // Assert
        salt.Should().BeEquivalentTo(new byte[]
        {
            1, 35, 69, 103, 137, 171, 205, 239,
            1, 35, 69, 103, 137, 171, 205, 239,
            1, 35, 69, 103, 137, 171, 205, 239,
            1, 35, 69, 103, 137, 171, 205, 239
        });
    }

    [Fact]
    public void Hash_WithFile_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{nameof(HashLite_WithFile_ReturnsExpected)}");

        // Act
        var result = fi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        result.Should().Be("72bb71f5d67dbdde008eb5331b3baec5");
    }

    [Fact]
    public void HashLite_WithFile_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestFiles", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello this is a slightly larger file.");

        // Act
        var result = fi.HashLite(HashType.Md5, 10, 2);

        // Assert
        result.Should().BeEquivalentTo(new byte[]
        {
            180, 30, 103, 199, 233, 171, 46, 76,
            96, 8, 95, 82, 185, 89, 12, 135
        });
    }
}
