// <copyright file="BlockStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Streams;

/// <summary>
/// Tests for the <see cref="BlockStream"/> class.
/// </summary>
public class BlockStreamTests
{
    [Fact]
    public void Ctor_WhenCalled_MatchesUnderlyingStream()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3]);

        // Act
        using var sut = new BlockStream(ms);
        sut.Position = 3;
        sut.SetLength(2);
        sut.Flush();

        // Assert
        sut.Id.Should().NotBeEmpty();
        sut.Position.Should().Be(2);
        sut.CanSeek.Should().Be(ms.CanSeek);
        sut.CanRead.Should().Be(ms.CanRead);
    }

    [Fact]
    public void SetCacheTrailer_WithWriteCache_FlushesCache()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        var bs = fi.OpenBlockWrite();
        bs.Write([9, 2, 4, 6, 7]);

        // Act
        bs.CacheTrailer = true;
        bs.Dispose();
        var md5Hex = File.ReadAllBytes(fi.FullName).Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.Should().Be("ca9c491ac66b2c62500882e93f3719a8");
    }

    [Fact]
    public void Dispose_WhenCalled_DisposesStream()
    {
        // Arrange
        var sut = new BlockStream(new MemoryStream([]));

        // Act
        sut.Dispose();
        var act = () => sut.Length;

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Read_UnusualBufferLength_DoesOk()
    {
        // Arrange
        const int oddBufferLength = 333;
        var testRef = Guid.NewGuid();
        var sourceFi = new FileInfo($"{testRef}_read-oddbuffersrc-sample.avi");
        var targetFi = new FileInfo($"{testRef}_read-oddbuffertrg-sample.avi");
        File.Copy("TestObjects/sample.avi", sourceFi.FullName);
        var sourceFs = sourceFi.OpenBlockRead(oddBufferLength);
        var targetFs = targetFi.OpenWrite();

        // Act
        sourceFs.CopyTo(targetFs, oddBufferLength);
        targetFs.Dispose();
        sourceFs.Dispose();
        var controlHash = sourceFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var testHash = targetFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        testHash.Should().Be(controlHash);
    }

    [Fact]
    public void Read_VariousPoints_MatchesDirect()
    {
        // Arrange
        const int bufLen = 4096;
        var testRef = Guid.NewGuid();
        var directFi = new FileInfo($"{testRef}_read-direct-sample.avi");
        var blocksFi = new FileInfo($"{testRef}_read-blocks-sample.avi");
        File.Copy("TestObjects/sample.avi", directFi.FullName);
        File.Copy("TestObjects/sample.avi", blocksFi.FullName);
        var directFs = directFi.OpenRead();
        var sutStream = blocksFi.OpenBlockRead();
        var buffer = new byte[bufLen];

        // Act
        var directHash1 = Md5Hex(directFs, buffer, 72, 3000);
        var blocksHash1 = Md5Hex(sutStream, buffer, 72, 3000);
        var directHash2 = Md5Hex(directFs, buffer, bufLen * 11, bufLen);
        var blocksHash2 = Md5Hex(sutStream, buffer, bufLen * 11, bufLen);
        var blocksHash3 = Md5Hex(directFs, buffer, sutStream.Length - 1000, bufLen);
        var directHash3 = Md5Hex(sutStream, buffer, sutStream.Length - 1000, bufLen);

        // Assert
        directHash1.Should().Be(blocksHash1);
        directHash2.Should().Be(blocksHash2);
        directHash3.Should().Be(blocksHash3);

        // Clean up
        sutStream.Dispose();
        directFs.Dispose();
        directFi.Delete();
        blocksFi.Delete();
    }

    [Fact]
    public void FlushCache_DirtyBlock_ThrowsExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        using var sut = new BlockStream(ms, bufferLength: 2);

        // Act
        sut.Seek(8, SeekOrigin.Begin);
        sut.CacheTrailer = true;
        sut.Seek(6, SeekOrigin.Begin);
        var act = () => sut.Write([1, 3]);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Unable to write dirty*");
    }

    [Fact]
    public void FinaliseWrite_MessedWithTrailer_ThrowsExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        using var sut = new BlockStream(ms, bufferLength: 2);

        // Act
        sut.Seek(6, SeekOrigin.Begin);
        sut.CacheTrailer = true;
        sut.Write([99, 22]);
        var act = sut.FinaliseWrite;

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Unexpected trailer*");
    }

    [Fact]
    public void FinaliseWrite_WithTrailer_DoesNotChangeLength()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        using var sut = fi.OpenBlockWrite(10);

        // Act
        sut.Write([6, 5, 4, 5, 6, 7, 3, 1]);
        sut.CacheTrailer = true;
        sut.Write([99, 22], 0, 2);
        var preLength = sut.Length;
        sut.FinaliseWrite();
        var postLength = sut.Length;

        // Assert
        preLength.Should().Be(postLength);
    }

    [Fact]
    public void Seek_WithWriteCache_HashesExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        using var sut = new BlockStream(ms, bufferLength: 2);

        // Act
        sut.Seek(6, SeekOrigin.Begin);
        sut.Write([99, 22]);
        sut.Seek(4, SeekOrigin.Begin);

        // Assert
        ms.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex).Should().Be("b1ba563dbce34007ca202aac966e4a32");
    }

    [Fact]
    public void Seek_AbandoningMidBlock_ThrowsExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        sut.Seek(3, SeekOrigin.Begin);
        sut.Write([1, 2]);

        var act = () => sut.Seek(4, SeekOrigin.Begin);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Unable to abandon block*");
    }

    [Fact]
    public void Seek_AbandoningPositionMatchesTrailerStart_ThrowsDifferentError()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        sut.Seek(5, SeekOrigin.Begin);
        sut.Write([1, 2]);
        sut.Write([1]);

        var act = () => sut.Seek(4, SeekOrigin.Begin);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Unable to write dirty*");
    }

    [Fact]
    public void Seek_AbandoningFirstBlock_HashesExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        sut.Seek(1, SeekOrigin.Begin);
        sut.Write([1]);
        sut.Seek(4, SeekOrigin.Begin);

        // Assert
        ms.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex).Should().Be("5c320e0fe97322a2cf80b60cc718b993");
    }

    [Fact]
    public void Seek_AbandoningInTrailer_HashesExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        sut.Seek(8, SeekOrigin.Begin);
        sut.Write([1]);
        sut.Seek(4, SeekOrigin.Begin);

        // Assert
        ms.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex).Should().Be("7dd0fd106ae0f94f162113a63c93d2fe");
    }

    [Fact]
    public void Seek_AbandoningNoTrailer_HashesExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.Seek(8, SeekOrigin.Begin);
        sut.Write([1]);
        sut.Seek(4, SeekOrigin.Begin);

        // Assert
        ms.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex).Should().Be("47ddbd444989967f6f17e3f1f7369975");
    }

    [Fact]
    public void Write_WithTrailer_ExtendsStream()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        var sut = fi.OpenBlockWrite();

        // Act
        sut.Write([1, 2, 3, 4, 5, 6, 7]);
        sut.CacheTrailer = true;
        sut.Write([4, 4, 4]);
        sut.Dispose();

        // Assert
        var bytes = File.ReadAllBytes(fi.FullName);
        bytes.Hash(HashType.Md5).Encode(Codec.ByteHex).Should().Be("a63c90cc3684ad8b0a2176a6a8fe9005");
    }

    [Fact]
    public void Write_UnusualBufferLength_DoesOk()
    {
        // Arrange
        const int oddBufferLength = 333;
        var testRef = Guid.NewGuid();
        var sourceFi = new FileInfo($"{testRef}_write-oddbuffersrc.pdf");
        var targetFi = new FileInfo($"{testRef}_write-oddbuffertrg.pdf");
        File.Copy("TestObjects/midsize.pdf", sourceFi.FullName);
        var sourceFs = sourceFi.OpenRead();
        var targetFs = targetFi.OpenBlockWrite(oddBufferLength);

        // Act
        sourceFs.CopyTo(targetFs, oddBufferLength);
        targetFs.FinaliseWrite();
        targetFs.Dispose();
        sourceFs.Dispose();
        var controlHash = sourceFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var testHash = targetFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        testHash.Should().Be(controlHash);
    }

    [Fact]
    public void Write_WithFinalise_MatchesDirect()
    {
        // Arrange
        const int bufLen = 32768;
        var testRef = Guid.NewGuid();
        var referenceFi = new FileInfo($"{testRef}_reference-file.avi");
        File.Copy("TestObjects/sample.avi", referenceFi.FullName);
        var referenceMd5 = referenceFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var blocksFi = new FileInfo($"{testRef}_write-blocks-new.avi");
        using var blocksFs = blocksFi.OpenWrite();
        using var sutStream = new BlockStream(blocksFs, bufLen);

        // Act
        using var refFs = referenceFi.OpenRead();
        refFs.CopyTo(sutStream, bufLen);
        sutStream.FinaliseWrite();

        refFs.Close();
        blocksFs.Close();
        sutStream.Close();
        var newMd5 = blocksFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        newMd5.Should().Be(referenceMd5);
        referenceFi.Delete();
        blocksFi.Delete();
    }

    private static string Md5Hex(Stream stream, byte[] buffer, long position, int count)
    {
        Array.Clear(buffer, 0, buffer.Length);
        stream.Seek(position, SeekOrigin.Begin);
        var read = stream.Read(buffer, 0, count);
        return buffer.AsSpan(0, read).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);
    }
}