// <copyright file="BlockStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Streams;
using CryptoStream.Tests.TestObjects;

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
        sut.Id.ShouldNotBeEmpty();
        sut.Position.ShouldBe(2);
        sut.CanSeek.ShouldBe(ms.CanSeek);
        sut.CanRead.ShouldBe(ms.CanRead);
    }

    [Fact]
    public void SetCacheTrailer_WhenCalled_CallsFlushCache()
    {
        // Arrange
        using var sut = new TestBlockStream();

        // Act
        Action act = () => sut.CacheTrailer = true;

        // Assert
        _ = act.ShouldThrow<NotSupportedException>();
    }

    [Fact]
    public void SetCacheTrailer_WithWriteCache_FlushesCache()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        var bs = fi.OpenBlockWrite(5);
        bs.Write([9, 2, 4, 6, 7]);

        // Act
        bs.CacheTrailer = true;
        bs.Write([9, 3, 4, 1, 1]);
        bs.FinaliseWrite();
        bs.Dispose();
        var md5Hex = File.ReadAllBytes(fi.FullName).Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.ShouldBe("4b5b8d20818fc108366ff1662ba819cc");
    }

    [Fact]
    public void SetCacheTrailer_WithWriteCacheAndSeek_FlushesCache()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        var bs = fi.OpenBlockWrite(5);
        bs.Write([9, 2, 4, 6, 7]);
        _ = bs.Seek(5, SeekOrigin.Begin);

        // Act
        bs.CacheTrailer = true;
        bs.Write([9, 3, 4, 1, 1]);
        bs.FinaliseWrite();
        bs.Dispose();
        var md5Hex = File.ReadAllBytes(fi.FullName).Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.ShouldBe("4b5b8d20818fc108366ff1662ba819cc");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void SetCacheTrailer_SeeksOnly_DoesNotWriteBytes(long position)
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        var bs = fi.OpenBlockWrite();
        bs.Write([9, 2, 4, 6, 7]);
        _ = bs.Seek(position, SeekOrigin.Begin);

        // Act
        bs.CacheTrailer = true;
        bs.Dispose();
        var md5Hex = File.ReadAllBytes(fi.FullName).Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        md5Hex.ShouldBe("db8c4ba99d15ab9463659798b1b9a385");
    }

    [Fact]
    public void Dispose_WhenCalled_DisposesStream()
    {
        // Arrange
        var sut = new BlockStream(new MemoryStream([]));

        // Act
        sut.Dispose();
        Action act = () => _ = sut.Length;

        // Assert
        _ = act.ShouldThrow<Exception>();
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
        testHash.ShouldBe(controlHash);
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
        var blocksHash4 = Md5Hex(directFs, buffer, 32000, 2000);
        var directHash4 = Md5Hex(sutStream, buffer, 32000, 2000);

        // Assert
        directHash1.ShouldBe(blocksHash1);
        directHash2.ShouldBe(blocksHash2);
        directHash3.ShouldBe(blocksHash3);
        directHash4.ShouldBe(blocksHash4);

        // Clean up
        sutStream.Dispose();
        directFs.Dispose();
        directFi.Delete();
        blocksFi.Delete();
    }

    [Fact]
    public void Read_StraddlingBlock_HashesExpected()
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
        var directHash1 = Md5Hex(directFs, buffer, 4000, 200);
        var blocksHash1 = Md5Hex(sutStream, buffer, 4000, 200);

        // Assert
        directHash1.ShouldBe(blocksHash1);

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
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.CacheTrailer = true;
        _ = sut.Seek(6, SeekOrigin.Begin);
        var act = () => sut.Write([1, 3]);

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldMatch("Unable to write dirty.*");
    }

    [Fact]
    public void FlushCache_TrailerExactStart_DoesNotThrow()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        using var sut = fi.OpenBlockWrite(2);

        // Act
        sut.Write([8, 4, 3, 2, 1, 5, 6, 9]);
        sut.CacheTrailer = true;
        _ = sut.Seek(2, SeekOrigin.Begin);
        var act = sut.FinaliseWrite;

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void FlushCache_TrailerExactEnd_DoesNotThrow()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        using var sut = fi.OpenBlockWrite(2);

        // Act
        sut.Write([8, 4, 3, 2, 1, 5]);
        sut.CacheTrailer = true;
        sut.Write([99, 88]);
        _ = sut.Seek(6, SeekOrigin.Begin);
        var act = sut.FinaliseWrite;

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void FlushCache_BodySeek_DoesNotThrow()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        using var sut = fi.OpenBlockWrite(2);

        // Act
        sut.Write([8, 4, 3, 2, 1, 5, 6, 9]);
        sut.CacheTrailer = true;
        _ = sut.Seek(4, SeekOrigin.Begin);
        var act = sut.FinaliseWrite;

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void FlushCache_DirtyHeaderWrite_DoesNotThrow()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        using var sut = fi.OpenBlockWrite(2);

        // Act
        sut.Write([1, 2, 3, 4, 5, 6]);
        sut.CacheTrailer = true;
        sut.Write([7, 7]);
        _ = sut.Seek(0, SeekOrigin.Begin);
        var act = () => sut.Write([8, 8]);

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void FlushCache_DirtyTrailerWrite_DoesNotThrow()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        using var sut = fi.OpenBlockWrite(2);

        // Act
        sut.Write([1, 2, 3, 4, 5, 6]);
        sut.CacheTrailer = true;
        sut.Write([7, 7]);
        _ = sut.Seek(6, SeekOrigin.Begin);
        sut.Write([8, 8]);
        var act = sut.FlushCache;

        // Assert
        act.ShouldNotThrow();
    }

    [Fact]
    public void FinaliseWrite_MessedWithTrailer_ThrowsExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        using var sut = new BlockStream(ms, bufferLength: 2);

        // Act
        _ = sut.Seek(6, SeekOrigin.Begin);
        sut.CacheTrailer = true;
        sut.Write([99, 22]);
        var act = sut.FinaliseWrite;

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldMatch("Unexpected trailer.*");
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
        preLength.ShouldBe(postLength);
    }

    [Fact]
    public void Seek_WithWriteCache_HashesExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9]);
        using var sut = new BlockStream(ms, bufferLength: 2);

        // Act
        _ = sut.Seek(6, SeekOrigin.Begin);
        sut.Write([99, 22]);
        _ = sut.Seek(4, SeekOrigin.Begin);

        // Assert
        ms.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex).ShouldBe("b1ba563dbce34007ca202aac966e4a32");
    }

    [Fact]
    public void Seek_AbandoningMidBlock_ThrowsExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        _ = sut.Seek(3, SeekOrigin.Begin);
        sut.Write([1, 2]);

        Action act = () => sut.Seek(4, SeekOrigin.Begin);

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldMatch("Unable to abandon block.*");
    }

    [Fact]
    public void Seek_AbandoningMidBlockButIsFirst_DoesNotThrow()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        _ = sut.Seek(0, SeekOrigin.Begin);
        sut.Write([1, 2]);

        var act = () => sut.Seek(4, SeekOrigin.Begin);

        // Assert
        _ = act.ShouldNotThrow();
    }

    [Fact]
    public void Seek_AbandoningPositionMatchesTrailerStart_ThrowsDifferentError()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        _ = sut.Seek(5, SeekOrigin.Begin);
        sut.Write([1, 2]);
        sut.Write([1]);

        Action act = () => sut.Seek(4, SeekOrigin.Begin);

        // Assert
        act.ShouldThrow<InvalidOperationException>().Message.ShouldMatch("Unable to write dirty.*");
    }

    [Fact]
    public void Seek_AbandoningFirstBlock_HashesExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        _ = sut.Seek(1, SeekOrigin.Begin);
        sut.Write([1]);
        _ = sut.Seek(4, SeekOrigin.Begin);

        // Assert
        ms.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex).ShouldBe("5c320e0fe97322a2cf80b60cc718b993");
    }

    [Fact]
    public void Seek_AbandoningInTrailer_HashesExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        sut.CacheTrailer = true;
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.Write([1]);
        _ = sut.Seek(4, SeekOrigin.Begin);

        // Assert
        ms.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex).ShouldBe("7dd0fd106ae0f94f162113a63c93d2fe");
    }

    [Fact]
    public void Seek_AbandoningNoTrailer_HashesExpected()
    {
        // Arrange
        var ms = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        using var sut = new BlockStream(ms, bufferLength: 4);

        // Act
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.Write([99]);
        _ = sut.Seek(8, SeekOrigin.Begin);
        sut.Write([1]);
        _ = sut.Seek(4, SeekOrigin.Begin);

        // Assert
        ms.ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex).ShouldBe("47ddbd444989967f6f17e3f1f7369975");
    }

    [Fact]
    public void Write_WithTrailer_ExtendsStream()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        var sut = fi.OpenBlockWrite(4);

        // Act
        sut.Write([1, 2, 3, 4, 5, 6, 7]);
        sut.CacheTrailer = true;
        sut.Write([4, 4, 4]);
        sut.FinaliseWrite();
        sut.Dispose();

        // Assert
        var bytes = File.ReadAllBytes(fi.FullName);
        bytes.Hash(HashType.Md5).Encode(Codec.ByteHex).ShouldBe("5d924de0db8f710fbd9fec0e4064eb47");
    }

    [Fact]
    public void Write_RewriteTrailer_HashesExpected()
    {
        // Arrange
        var fi = new FileInfo($"{Guid.NewGuid()}.txt");
        var sut = fi.OpenBlockWrite(4);

        // Act
        sut.Write([1, 2, 3, 4, 5, 6, 7]);
        sut.CacheTrailer = true;
        sut.Write([4, 4, 4]);
        _ = sut.Seek(4, SeekOrigin.Begin);
        sut.Write([5, 5, 5]);
        sut.FinaliseWrite();
        sut.Dispose();

        // Assert
        var bytes = File.ReadAllBytes(fi.FullName);
        bytes.Hash(HashType.Md5).Encode(Codec.ByteHex).ShouldBe("aa215cd0501a79d649596385acca7450");
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
        testHash.ShouldBe(controlHash);
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
        newMd5.ShouldBe(referenceMd5);
        referenceFi.Delete();
        blocksFi.Delete();
    }

    private static string Md5Hex(Stream stream, byte[] buffer, long position, int count)
    {
        Array.Clear(buffer, 0, buffer.Length);
        _ = stream.Seek(position, SeekOrigin.Begin);
        var read = stream.Read(buffer, 0, count);
        return buffer.AsSpan(0, read).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);
    }
}