﻿// <copyright file="FileExtensionsTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.IO;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Tests.TestObjects;
using CryptoStream.Transform;

/// <summary>
/// Tests for <see cref="FileExtensions"/>.
/// </summary>
public class FileExtensionsTests
{
    [Fact]
    public void DecryptHere_WhenCalled_ReturnsMetadata()
    {
        // Arrange
        var originalName = $"{Guid.NewGuid()}.txt";
        var fi = new FileInfo(Path.Combine("TestObjects", originalName));
        File.WriteAllText(fi.FullName, fi.Name);
        _ = fi.EncryptInSitu(TestRefs.TestKey);

        // Act
        var result = fi.DecryptHere(TestRefs.TestKey);

        // Assert
        result.Name.ShouldMatch($"{fi.Name[..12]}.*.txt");
        result.Exists.ShouldBeTrue();
    }

    [Fact]
    public void DecryptTo_WhenCalled_ReturnsMetadata()
    {
        // Arrange
        var originalName = $"{Guid.NewGuid()}.txt";
        var fi = new FileInfo(Path.Combine("TestObjects", originalName));
        File.WriteAllText(fi.FullName, fi.Name);
        _ = fi.EncryptInSitu(TestRefs.TestKey);
        var trgStream = new MemoryStream();

        // Act
        var result = fi.DecryptTo(trgStream, TestRefs.TestKey);

        // Assert
        result["filename"].ShouldBe(originalName);
    }

    [Fact]
    public void DecryptTo_WithDecryptor_CallsDecrypt()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{TestRefs.CryptoFileName}.{Guid.NewGuid()}"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        var mockDecryptor = new Mock<IDecryptor>();
        var trgStream = new MemoryStream();

        // Act
        _ = fi.DecryptTo(trgStream, TestRefs.TestKey, mockDecryptor.Object);

        // Assert
        mockDecryptor.Verify(
            m => m.Decrypt(It.IsAny<Stream>(), trgStream, TestRefs.TestKey, It.IsAny<byte[]>(), true, 32768, null),
            Times.Once());
    }

    [Fact]
    public void DecryptTo_DefaultDecryptor_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        var content = $"hi{Guid.NewGuid()}";
        File.WriteAllText(fi.FullName, content);
        _ = fi.EncryptInSitu(TestRefs.TestKey);
        var trgStream = new MemoryStream();

        // Act
        _ = fi.DecryptTo(trgStream, TestRefs.TestKey);

        // Assert
        trgStream.ToArray().Encode(Codec.CharUtf8).ShouldBe(content);
    }

    [Fact]
    public void DecryptTo_UndersizedBuffer_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        var content = $"hi{Guid.NewGuid()}";
        File.WriteAllText(fi.FullName, content);
        _ = fi.EncryptInSitu(TestRefs.TestKey, bufferLength: 12);
        var trgStream = new MemoryStream();

        // Act
        _ = fi.DecryptTo(trgStream, TestRefs.TestKey, bufferLength: 12);

        // Assert
        trgStream.ToArray().Encode(Codec.CharUtf8).ShouldBe(content);
    }

    [Fact]
    public void DecryptTo_WithMac_ReturnsExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        var content = $"hi{Guid.NewGuid()}";
        File.WriteAllText(fi.FullName, content);
        using var macStream = new MemoryStream();
        _ = fi.EncryptInSitu(TestRefs.TestKey, mac: macStream);
        var trgStream = new MemoryStream();

        // Act
        _ = fi.DecryptTo(trgStream, TestRefs.TestKey, mac: macStream);

        // Assert
        trgStream.ToArray().Encode(Codec.CharUtf8).ShouldBe(content);
    }

    [Fact]
    public void EncryptInSitu_AlreadyEncrypted_ThrowsArgumentException()
    {
        // Arrange
        var fi = new FileInfo(
            Path.Combine(
                "TestObjects",
                "2fbdd1cbdb5f317b7e21ebb7ae7c32d166feec3be76b64d470123bf4d2c06ae5.03470a9848"));

        // Act
        var act = () => fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldMatch("File.*already.*secure.*");
    }

    [Theory]
    [InlineData("33,4,33,2,233,1", "38cbe01028")]
    [InlineData("9,0,2,1,0", "55338850dc")]
    public void EncryptInSitu_WithSameContent_SaltIsDeterministic(string keyBytesCsv, string expectedStart)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{nameof(this.EncryptInSitu_WithSameContent_SaltIsDeterministic)}");
        var keyBytes = keyBytesCsv.Split(',').Select(byte.Parse).ToArray();

        // Act
        var salt = fi.EncryptInSitu(keyBytes);

        // Assert
        salt.ShouldStartWith(expectedStart);
    }

    [Fact]
    public void EncryptInSitu_WithNullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        _ = act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void EncryptInSitu_WithFile_UpdatesFileInfoReference()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello, world");

        // Act
        var salt = fi.EncryptInSitu(TestRefs.TestKey);

        // Assert
        fi.Name.ShouldBe(salt + ".20ab58c82d");
        fi.Exists.ShouldBeTrue();
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
        macStream.ToArray().Length.ShouldBe(16);
    }

    [Theory]
    [InlineData(TestRefs.CryptoFileName + TestRefs.CryptoFileExt, true)]
    [InlineData(TestRefs.CryptoFileName + ".txt", false)]
    [InlineData("other-junk." + TestRefs.CryptoFileName + TestRefs.CryptoFileExt, true)]
    [InlineData(TestRefs.CryptoFileName + "e", false)]
    [InlineData(TestRefs.CryptoFileName + TestRefs.CryptoFileExt + "e", false)]
    public void IsSecure_VaryingFormat_ReturnsExpected(string name, bool expected)
    {
        // Arrange
        var fi = new FileInfo(name);

        // Act
        var result = fi.IsSecure();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void IsSecure_NullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        Action act = () => _ = fi.IsSecure();

        // Assert
        _ = act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void ToSalt_NullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = fi.ToSalt;

        // Assert
        _ = act.ShouldThrow<ArgumentNullException>();
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
        var act = fi.ToSalt;

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe($"Unable to obtain salt: '{notASalt}' (Parameter 'fi')");
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
        salt.ShouldBeEquivalentTo(new byte[]
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
        result.ShouldBe("72bb71f5d67dbdde008eb5331b3baec5");
    }

    [Fact]
    public void Hash_NullStream_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => fi.Hash(HashType.Md5);

        // Assert
        _ = act.ShouldThrow<ArgumentNullException>();
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
        result.ShouldBeEquivalentTo(new byte[]
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
        _ = act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void ToSecureExtension_NullParam_ThrowsException()
    {
        // Arrange
        var file = new FileInfo(new string('a', 64));

        // Act
        var act = () => file.ToSecureExtension(null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("plainExtension");
    }

    [Theory]
    [InlineData("")]
    [InlineData("..")]
    [InlineData(".length")]
    public void ToSecureExtension_InvalidPlainExtension_ThrowsException(string badExtension)
    {
        // Arrange
        var file = new FileInfo(new string('a', 64));

        // Act
        var act = () => file.ToSecureExtension(badExtension);

        // Assert
        act.ShouldThrow<ArgumentException>().ShouldSatisfyAllConditions(
            ex => ex.ParamName.ShouldBe("plainExtension"),
            ex => ex.Message.ShouldMatch("Unable to parse file data.*"));
    }

    [Theory]
    [InlineData('a', ".4bf0265e44")]
    [InlineData('9', ".774c75440c")]
    public void ToSecureExtension_ValidParams_ReturnsExpected(char repeatingChar, string expected)
    {
        // Arrange
        var file = new FileInfo(new string(repeatingChar, 64));

        // Act
        var result = file.ToSecureExtension(".avi");

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void ToSecureExtension_WithEncryptor_EncryptsBlock()
    {
        // Arrange
        var mockEncryptor = new Mock<IGcmEncryptor>();
        _ = mockEncryptor
            .Setup(m => m.EncryptBlock(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns(new GcmEncryptedBlock([], []));
        var file = new FileInfo(new string('a', 64));

        // Act
        _ = file.ToSecureExtension(".avi", mockEncryptor.Object);

        // Assert
        mockEncryptor.Verify(m => m.EncryptBlock(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData(".70010566666")]
    [InlineData(".nothex")]
    [InlineData(".700540172")]
    public void ToPlainExtension_InvalidSecureExtension_ThrowsException(string badExtension)
    {
        // Arrange
        var file = new FileInfo(new string('a', 64) + badExtension);

        // Act
        var act = () => file.ToPlainExtension();

        // Assert
        act.ShouldThrow<ArgumentException>().ShouldSatisfyAllConditions(
            ex => ex.ParamName.ShouldBe("secure"),
            ex => ex.Message.ShouldMatch("Unable to parse file data.*"));
    }

    [Theory]
    [InlineData('a', ".4bf0265e44")]
    [InlineData('9', ".774c75440c")]
    public void ToPlainExtension_ValidParams_ReturnsExpected(char repeatingChar, string extension)
    {
        // Arrange
        var file = new FileInfo(new string(repeatingChar, 64) + extension);

        // Act
        var result = file.ToPlainExtension();

        // Assert
        result.ShouldBe(".avi");
    }

    [Fact]
    public void ToPlainExtension_WithDecryptor_DecryptsBlock()
    {
        // Arrange
        var mockDecryptor = new Mock<IGcmDecryptor>();
        _ = mockDecryptor
            .Setup(m => m.DecryptBlock(It.IsAny<GcmEncryptedBlock>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), false))
            .Returns([]);
        var file = new FileInfo(new string('a', 64) + ".4bf0265e44");

        // Act
        _ = file.ToPlainExtension(mockDecryptor.Object);

        // Assert
        mockDecryptor.Verify(
            m => m.DecryptBlock(
                It.IsAny<GcmEncryptedBlock>(),
                It.IsAny<byte[]>(),
                It.IsAny<byte[]>(),
                false));
    }
}
