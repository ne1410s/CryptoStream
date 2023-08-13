// <copyright file="FileExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;
using Crypt.Tests.TestObjects;
using Crypt.Transform;

namespace Crypt.Tests.IO;

/// <summary>
/// Tests for <see cref="FileExtensions"/>.
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void DecryptTo_WithDecryptor_CallsDecrypt()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{TestRefs.CryptoFileName}.{Guid.NewGuid()}"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        var mockDecryptor = new Mock<IDecryptor>();
        var trgStream = new MemoryStream();

        // Act
        fi.DecryptTo(trgStream, TestRefs.TestKey, mockDecryptor.Object);

        // Assert
        mockDecryptor.Verify(
            m => m.Decrypt(It.IsAny<Stream>(), trgStream, TestRefs.TestKey, It.IsAny<byte[]>(), 32768, null),
            Times.Once());
    }

    [Fact]
    public void DecryptTo_DefaultDecryptor_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
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
    public void DecryptTo_UndersizedBuffer_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        var content = $"hi{Guid.NewGuid()}";
        File.WriteAllText(fi.FullName, content);
        fi.EncryptInSitu(TestRefs.TestKey, bufferLength: 12);
        var trgStream = new MemoryStream();

        // Act
        fi.DecryptTo(trgStream, TestRefs.TestKey, bufferLength: 12);

        // Assert
        trgStream.ToArray().Encode(Codec.CharUtf8).Should().Be(content);
    }

    [Fact]
    public void DecryptTo_WithMac_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
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
    public void EncryptInSitu_AlreadyEncrypted_ThrowsArgumentException()
    {
        // Arrange
        var fi = new FileInfo(
            Path.Combine(
                "TestObjects",
                "0f5bed56f862512644ec87b7db6afc7299e2195c5bf9b27bcc631adb16785ed9.avi"));

        // Act
        var act = () => fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("File*already*secure*");
    }

    [Fact]
    public void EncryptInSitu_WithUpperCaseExt_SavesAsLower()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.TXT"));
        File.WriteAllText(fi.FullName, $"hi{nameof(this.EncryptInSitu_WithUpperCaseExt_SavesAsLower)}");

        // Act
        fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        fi.Name.Should().EndWith(".txt");
    }

    [Fact]
    public void EncryptInSitu_WithSameContent_SaltIsDeterministic()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{nameof(this.EncryptInSitu_WithSameContent_SaltIsDeterministic)}");

        // Act
        var salt = fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        salt.Should().Be("8062285b5b7c71eb689abea4c08889fc1ba18bb1726c75313c6966a7b968f21f");
    }

    [Fact]
    public void EncryptInSitu_WithNullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EncryptInSitu_WithFile_UpdatesFileInfoReference()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
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
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        using var macStream = new MemoryStream();

        // Act
        _ = fi.EncryptInSitu(TestRefs.TestKey, mac: macStream);

        // Assert
        macStream.ToArray().Length.Should().Be(16);
    }

    [Theory]
    [InlineData(TestRefs.CryptoFileName, true)]
    [InlineData(TestRefs.CryptoFileName + ".txt", true)]
    [InlineData("other-junk." + TestRefs.CryptoFileName + ".txt", true)]
    [InlineData(TestRefs.CryptoFileName + "e", false)]
    public void IsSecure_VaryingFormat_ReturnsExpected(string name, bool expected)
    {
        // Arrange
        var fi = new FileInfo(name);

        // Act
        var result = fi.IsSecure();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsSecure_NullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = fi.IsSecure;

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToSalt_NullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = fi.ToSalt;

        // Assert
        act.Should().Throw<ArgumentNullException>();
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
            .WithMessage($"Unable to obtain salt: '{notASalt}' (Parameter 'fi')");
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
            1, 35, 69, 103, 137, 171, 205, 239,
        });
    }

    [Fact]
    public void Hash_WithFile_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{nameof(this.HashLite_WithFile_ReturnsExpected)}");

        // Act
        var result = fi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        result.Should().Be("72bb71f5d67dbdde008eb5331b3baec5");
    }

    [Fact]
    public void Hash_NullStream_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => fi.Hash(HashType.Md5);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HashLite_WithFile_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello this is a slightly larger file.");

        // Act
        var result = fi.HashLite(HashType.Md5, 10, 2);

        // Assert
        result.Should().BeEquivalentTo(new byte[]
        {
            180, 30, 103, 199, 233, 171, 46, 76,
            96, 8, 95, 82, 185, 89, 12, 135,
        });
    }

    [Fact]
    public void HashLite_NullStream_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => fi.HashLite(HashType.Md5);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
