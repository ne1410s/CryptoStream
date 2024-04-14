// <copyright file="AesGcmEncryptorTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Tests.Transform;

using Crypt.Encoding;
using Crypt.Keying;
using Crypt.Tests.TestObjects;
using Crypt.Transform;
using Crypt.Utils;

/// <summary>
/// Test for the <see cref="AesGcmEncryptor"/>.
/// </summary>
public class AesGcmEncryptorTests
{
    [Fact]
    public void GenerateSalt_WithStream_ReturnsExpected()
    {
        // Arrange
        var key = new byte[] { 1, 2, 3 };
        var sut = new AesGcmEncryptor();
        using var stream = new MemoryStream(key);

        // Act
        var salt = sut.GenerateSalt(stream, key);

        // Assert
        salt.Encode(Codec.ByteHex).Should().Be("d6cffa25a09717f8f92c8230f55d4b846cafa1e8347b775fdbc3d4f58cdef533");
    }

    [Fact]
    public void GeneratePepper_WhenCalled_NoZeroBytes()
    {
        // Arrange
        var sut = new AesGcmEncryptor();

        var pepper = sut.GeneratePepper(null);

        // Assert
        pepper.Should().NotContain(default(byte));
    }

    [Fact]
    public void Encrypt_NullSource_ThrowsException()
    {
        // Arrange
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmEncryptor(resizer: mockResizer.Object);
        var srcStream = (Stream)null!;
        using var trgStream = new MemoryStream();

        // Act
        var act = () => sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, []);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [Fact]
    public void Encrypt_NullTarget_ThrowsException()
    {
        // Arrange
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmEncryptor(resizer: mockResizer.Object);
        using var srcStream = new MemoryStream();
        var trgStream = (Stream)null!;

        // Act
        var act = () => sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, []);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("target");
    }

    [Fact]
    public void Encrypt_OversizedTarget_GetsResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!!");
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmEncryptor(resizer: mockResizer.Object);
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, []);

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, 5),
            Times.Once);
    }

    [Fact]
    public void Encrypt_EqualSizedTarget_NotResized()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!!");
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmEncryptor(resizer: mockResizer.Object);
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, [], bufferLength: 5);

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public void Encrypt_WithKeyDeriver_CallsDerive()
    {
        // Arrange
        var mockDeriver = new Mock<ICryptoKeyDeriver>();
        var key = new byte[] { 99 };
        mockDeriver
            .Setup(m => m.DeriveCryptoKey(key, It.IsAny<byte[]>(), It.IsAny<byte[]>()))
            .Returns(Guid.NewGuid().ToByteArray());
        var sut = new AesGcmEncryptor(mockDeriver.Object);
        using var stream = new MemoryStream([1, 2, 3]);

        // Act
        _ = sut.Encrypt(stream, new MemoryStream(), key, []);

        // Assert
        mockDeriver.Verify(
            m => m.DeriveCryptoKey(key, It.IsAny<byte[]>(), It.IsAny<byte[]>()));
    }

    [Fact]
    public void Encrypt_ExcessiveMeta_ThrowsException()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!!");
        var sut = new AesGcmEncryptor();
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();
        var meta = new Dictionary<string, string>() { ["test"] = new string('x', 5000) };

        // Act
        var act = () => sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, meta);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Unexpected padding.");
    }

    [Fact]
    public void Encrypt_ValidSource_NonZeroPadding()
    {
        // Arrange
        var sut = new AesGcmEncryptor();
        using var srcStream = new MemoryStream([ 1, 2, 3]);
        using var trgStream = new MemoryStream();
        var checkBackBuffer = new byte[300];

        // Act
        sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, []);

        // Assert
        trgStream.Seek(-(4096 + checkBackBuffer.Length), SeekOrigin.End);
        trgStream.Read(checkBackBuffer);
        checkBackBuffer.Should().NotContain(default(byte));
    }

    [Fact]
    public void Encrypt_ValidSource_TargetLengthExpected()
    {
        // Arrange
        var sut = new AesGcmEncryptor();
        using var srcStream = new MemoryStream([1, 2, 3]);
        using var trgStream = new MemoryStream();

        // Act
        sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, []);

        // Assert
        trgStream.Length.Should().Be(5000);
    }

    [Fact]
    public void Encrypt_NullMeta_ProcessedOk()
    {
        // Arrange
        var fi = new FileInfo(Path.Combine("TestObjects", $"{Guid.NewGuid()}.txt"));
        File.WriteAllText(fi.FullName, "hi!!!");
        var sut = new AesGcmEncryptor();
        using var srcStream = fi.OpenRead();
        using var trgStream = new MemoryStream();

        // Act
        var result = sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, null).Encode(Codec.ByteHex);

        // Assert
        result.Should().Be("4b16d4eb3ab56591d6bc35d4a50d9cf718b79c547e84b2c2de2095378779535a");
    }
}
