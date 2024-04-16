// <copyright file="CryptoBlockReadStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Tests.Streams;

using Crypt.Encoding;
using Crypt.Hashing;
using Crypt.IO;
using Crypt.Streams;
using Crypt.Tests.TestObjects;
using Crypt.Transform;

/// <summary>
/// Tests for the <see cref="CryptoBlockReadStream"/>.
/// </summary>
public class CryptoBlockReadStreamTests
{
    [Fact]
    public void Ctor_WithDecryptor_CallsReadPepper()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hello here is another string");
        fi.EncryptInSitu(TestRefs.TestKey);
        var mockDecryptor = new Mock<IGcmDecryptor>();

        // Act
        using var str = new CryptoBlockReadStream(fi, TestRefs.TestKey, decryptor: mockDecryptor.Object);

        // Assert
        long param1;
        Dictionary<string, string> param2;
        mockDecryptor.Verify(
            m => m.ReadPepper(It.IsAny<Stream>(), It.IsAny<byte[]>(), out param1, out param2),
            Times.Once);
    }

    [Fact]
    public void Ctor_NullFile_ThrowsException()
    {
        // Arrange
        var fi = (FileInfo)null!;

        // Act
        var act = () => new CryptoBlockReadStream(fi, TestRefs.TestKey);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(327680)]
    [InlineData(3370848)]
    [InlineData(3384800)]
    public void Read_VaryingStartPosition_MimicsNonBlockingAuthority(long position, int bufferLength = 32768)
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", "sample.avi"));
        using var authority = new SimpleFileStream(fi, bufferLength);
        authority.Seek(position, SeekOrigin.Begin);
        var authBuffer = new byte[bufferLength];
        var authRead = authority.Read(authBuffer, 0, bufferLength);
        if (authRead < bufferLength)
        {
            Array.Resize(ref authBuffer, authRead);
        }

        var authMd5Hex = authBuffer.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var secureFi = new FileInfo(
            Path.Combine("TestObjects", "2fbdd1cbdb5f317b7e21ebb7ae7c32d166feec3be76b64d470123bf4d2c06ae5.03470a9848"));
        using var sut = new CryptoBlockReadStream(secureFi, TestRefs.TestKey);
        sut.Seek(position, SeekOrigin.Begin);

        // Act
        var result = sut.Read();
        var resultMd5Hex = result.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        resultMd5Hex.Should().Be(authMd5Hex);
        sut.Position.Should().Be(authority.Position);
    }

    [Fact]
    public void Read_SpanTwoBlocks_DecryptsTwoBlocks()
    {
        // Arrange
        const int bufferLength = 16;
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "this is a sentence more than twelve bytes for sure!");
        fi.EncryptInSitu(TestRefs.TestKey, bufferLength: bufferLength);
        var salt = fi.ToSalt();
        var mockDecryptor = new Mock<IGcmDecryptor>();
        var originalLength = 5000L;
        Dictionary<string, string> metadata;
        mockDecryptor
            .Setup(m => m.ReadPepper(It.IsAny<Stream>(), It.IsAny<byte[]>(), out originalLength, out metadata))
            .Returns(new byte[] { 1 }.Hash(HashType.Md5));
        mockDecryptor
            .Setup(m => m.DecryptBlock(It.IsAny<GcmEncryptedBlock>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), false))
            .Returns((GcmEncryptedBlock eb, byte[] _, byte[] _, bool _) => new byte[eb.MessageBuffer.Length]);
        using var stream = fi.OpenRead();
        using var sut = new CryptoBlockReadStream(stream, salt, TestRefs.TestKey, bufferLength, mockDecryptor.Object);
        sut.Seek(12);

        // Act
        _ = sut.Read();

        // Assert
        mockDecryptor.Verify(
            m => m.DecryptBlock(It.IsAny<GcmEncryptedBlock>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), false),
            Times.Exactly(2));
    }

    [Fact]
    public void Props_WhenPopulated_ShouldBeExpected()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, $"hi{Guid.NewGuid()}");
        fi.EncryptInSitu(TestRefs.TestKey);
        var salt = fi.ToSalt();
        using var stream = fi.OpenRead();
        using var sut = new CryptoBlockReadStream(stream, salt, TestRefs.TestKey);

        // Act
        var uri = sut.Uri;
        var length = sut.Length;

        // Assert
        uri.Should().NotBeEmpty();
        length.Should().Be(38);
    }
}
