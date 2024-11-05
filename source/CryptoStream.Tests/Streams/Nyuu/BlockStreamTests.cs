﻿// <copyright file="BlockStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams.Nyuu;

using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Streams.Nyuu;

/// <summary>
/// Tests for the <see cref="BlockStream"/> class.
/// </summary>
public class BlockStreamTests
{
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
        using var directFs = directFi.OpenRead();
        using var blocksFs = blocksFi.OpenRead();
        using var sutStream = new BlockStream(blocksFs, bufLen);
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
        directFs.Close();
        blocksFs.Close();
        directFi.Delete();
        blocksFi.Delete();
    }

    [Fact]
    public void Write_WithFinalise_MatchesDirect()
    {
        // Arrange
        const int bufLen = 333;
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
