// <copyright file="AesGcmDecryptorTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Transform;

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
        var act = () => sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt);

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
        var act = () => sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt);

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
        var act = () => sut.ReadPepper(srcStream, [], out _, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
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
        sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt);

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
        var result = sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt);

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
        sut.Decrypt(srcStream, trgStream, TestRefs.TestKey, salt, bufferLength);

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
        var block = (GcmEncryptedBlock)null!;

        // Act
        var act = () => sut.DecryptBlock(block, [], [], true);

        // Assert
        act.Should().Throw<ArgumentNullException>();
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
        var act = () => sut.Decrypt(target, new MemoryStream(), userKey, [2]);

        // Assert
        act.Should().Throw<Exception>();
        mockDeriver.Verify(
            m => m.DeriveCryptoKey(userKey, It.IsAny<byte[]>(), It.IsAny<byte[]>()));
    }
}
