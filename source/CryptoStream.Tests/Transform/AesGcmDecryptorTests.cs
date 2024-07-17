// <copyright file="AesGcmDecryptorTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Transform;

using System.Security.Cryptography;
using CryptoStream.IO;
using CryptoStream.Keying;
using CryptoStream.Tests.TestObjects;
using CryptoStream.Transform;
using CryptoStream.Utils;

/// <summary>
/// Tests for the <see cref="AesGcmDecryptor"/>.
/// </summary>
public class AesGcmDecryptorTests
{
    [Fact]
    public void DecryptTo_OversizedTarget_GetsResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hey!");
        fi.EncryptInSitu(TestRefs.TestKey);
        using var trgStream = new MemoryStream(Enumerable.Repeat((byte)1, 20).ToArray());

        // Act
        fi.DecryptTo(trgStream, TestRefs.TestKey);

        // Assert
        trgStream.Length.Should().Be(4);
    }

    [Fact]
    public void Decrypt_NullSource_ThrowsException()
    {
        // Arrange
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmDecryptor(resizer: mockResizer.Object);
        var salt = new byte[] { 1, 2, 3 };
        var srcStream = (Stream)null!;
        using var trgStream = new MemoryStream();

        // Act
        var act = () => sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt, null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [Fact]
    public void Decrypt_NullTarget_ThrowsException()
    {
        // Arrange
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmDecryptor(resizer: mockResizer.Object);
        var salt = new byte[] { 1, 2, 3 };
        using var srcStream = new MemoryStream();
        var trgStream = (Stream)null!;

        // Act
        var act = () => sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt, null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("target");
    }

    [Fact]
    public void ReadPepper_NullSource_ThrowsException()
    {
        // Arrange
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmDecryptor(resizer: mockResizer.Object);
        var srcStream = (Stream)null!;

        // Act
        var act = () => sut.ReadPepper(srcStream, [], null, out _, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ReadPepper_BadKey_ThrowsException()
    {
        // Arrange
        var sut = new AesGcmDecryptor();
        var srcStream = new MemoryStream([1, 2, 3]);
        var trgStream = new MemoryStream();
        new AesGcmEncryptor().Encrypt(srcStream, trgStream, TestRefs.TestKey, new());
        var badKey = new byte[] { 9, 9, 0 };

        // Act
        var act = () => sut.ReadPepper(trgStream, badKey, null, out _, out _);

        // Assert
        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void ReadPepper_AtMetadataBufferThreshold_DoesNotThrow()
    {
        // Arrange
        var sut = new AesGcmDecryptor();
        var buf = new byte[4096];
        Array.Fill(buf, (byte)4);
        var srcStream = new MemoryStream(buf);
        var trgStream = new MemoryStream();
        new AesGcmEncryptor().Encrypt(srcStream, trgStream, TestRefs.TestKey, new());
        var encrypted = trgStream.ToArray();
        var x = new Span<byte>(encrypted, encrypted.Length - 128, 128);
        var sutBuf = new byte[4096 - 128].Concat(x.ToArray()).ToArray();
        trgStream = new MemoryStream(sutBuf);

        // Act
        var act = () => sut.ReadPepper(trgStream, TestRefs.TestKey, null, out _, out _);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ReadPepper_BadMetadataIndicator_ThrowsIOException()
    {
        // Arrange
        var sut = new AesGcmDecryptor();
        var srcStream = new MemoryStream([1, 2, 3]);
        var trgStream = new MemoryStream();
        new AesGcmEncryptor().Encrypt(srcStream, trgStream, TestRefs.TestKey, new());

        // Act
        var act = () => sut.ReadPepper(trgStream, TestRefs.TestKey, true, out _, out _);

        // Assert
        act.Should().Throw<IOException>();
    }

    [Fact]
    public void Decrypt_OversizedTarget_CallsResize()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!");
        fi.EncryptInSitu(TestRefs.TestKey);
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmDecryptor(resizer: mockResizer.Object);
        var salt = fi.ToSalt();
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt, true);

        // Assert
        trgStream.Position.Should().Be(0);
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, 4),
            Times.Once);
    }

    [Fact]
    public void Decrypt_ValidSource_ReturnExpectedMetadata()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        var expected = new Dictionary<string, string>() { ["filename"] = fi.Name };
        File.WriteAllText(fi.FullName, "hi!!");
        fi.EncryptInSitu(TestRefs.TestKey);
        var sut = new AesGcmDecryptor();
        var salt = fi.ToSalt();
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        var result = sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt, true);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Decrypt_EqualSizedTarget_NotResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!");
        const int bufferLength = 4;
        fi.EncryptInSitu(TestRefs.TestKey, bufferLength: bufferLength);
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmDecryptor(resizer: mockResizer.Object);
        var salt = fi.ToSalt();
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt, true, bufferLength);

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public void DecryptBlock_NullBlock_ThrowsExpected()
    {
        // Arrange
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmDecryptor(resizer: mockResizer.Object);

        // Act
        var act = () => sut.DecryptBlock(null!, [], [], default);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DecryptBlock_AuthenticateBadMacBuffer_ReturnsExpected()
    {
        // Arrange
        var sut = new AesGcmDecryptor();
        var key = new byte[32];
        var tag = new byte[16];
        var block = new GcmEncryptedBlock([71, 120, 194, 33], tag);

        // Act
        var act = () => sut.DecryptBlock(block, key, 1L.RaiseBits(), true);

        // Assert
        act.Should().Throw<AuthenticationTagMismatchException>();
    }

    [Fact]
    public void Decrypt_WithKeyDeriver_CallsDerive()
    {
        // Arrange
        var userKey = new byte[] { 1, 2, 3 };
        var mockDeriver = new Mock<ICryptoKeyDeriver>();
        var sut = new AesGcmDecryptor(mockDeriver.Object);
        using var stream = new MemoryStream([1, 2, 3]);
        using var target = new MemoryStream();
        new AesGcmEncryptor().Encrypt(stream, target, userKey, []);

        // Act
        var act = () => sut.Decrypt(target, new MemoryStream(), userKey, [2], false);

        // Assert
        act.Should().Throw<Exception>();
        mockDeriver.Verify(
            m => m.DeriveCryptoKey(userKey, It.IsAny<byte[]>(), It.IsAny<byte[]>()));
    }
}
