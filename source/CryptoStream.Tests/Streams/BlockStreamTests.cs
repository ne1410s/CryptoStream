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
        var sut = new BlockStream(ms);
        sut.Position = 3;
        sut.SetLength(2);
        sut.Flush();

        // Assert
        sut.Position.Should().Be(2);
        sut.CanSeek.Should().Be(ms.CanSeek);
        sut.CanRead.Should().Be(ms.CanRead);
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