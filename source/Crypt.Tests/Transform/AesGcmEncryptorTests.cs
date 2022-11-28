// <copyright file="AesGcmEncryptorTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

using Crypt.Encoding;
using Crypt.Tests.TestObjects;
using Crypt.Transform;
using Crypt.Utils;

namespace Crypt.Tests.Transform;

/// <summary>
/// Test for the <see cref="AesGcmEncryptor"/>.
/// </summary>
public class AesGcmEncryptorTests
{
    [Fact]
    public void GenerateSalt_WithStream_ReturnsExpected()
    {
        // Arrange
        var sut = new AesGcmEncryptor();
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var salt = sut.GenerateSalt(stream);

        // Assert
        salt.Encode(Codec.ByteHex).Should().Be("5890032c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81c6f2c0cb49");
    }

    [Fact]
    public void Encrypt_NullInput_ThrowsException()
    {
        // Arrange
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmEncryptor(resizer: mockResizer.Object);
        var srcStream = (Stream)null!;
        using var trgStream = new MemoryStream();

        // Act
        var act = () => sut.Encrypt(srcStream, trgStream, TestRefs.TestKey);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("input");
    }

    [Fact]
    public void Encrypt_NullOutput_ThrowsException()
    {
        // Arrange
        var mockResizer = new Mock<IArrayResizer>();
        var sut = new AesGcmEncryptor(resizer: mockResizer.Object);
        using var srcStream = new MemoryStream();
        var trgStream = (Stream)null!;

        // Act
        var act = () => sut.Encrypt(srcStream, trgStream, TestRefs.TestKey);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("output");
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
        sut.Encrypt(srcStream, trgStream, TestRefs.TestKey);

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
        sut.Encrypt(srcStream, trgStream, TestRefs.TestKey, bufferLength: 5);

        // Assert
        mockResizer.Verify(
            m => m.Resize(ref It.Ref<byte[]>.IsAny, It.IsAny<int>()),
            Times.Never);
    }
}
