// <copyright file="GcmCryptoStreamTests.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream.Tests.Streams;

using System.Security.Cryptography;
using CryptoStream.Encoding;
using CryptoStream.Hashing;
using CryptoStream.IO;
using CryptoStream.Streams;
using CryptoStream.Tests.TestObjects;

/// <summary>
/// Tests for the <see cref="GcmCryptoStream"/> class.
/// </summary>
public class GcmCryptoStreamTests
{
    [Fact]
    public void FinaliseWrite_WhenCalled_WritesNonZeros()
    {
        // Arrange
        var salt = Guid.NewGuid().ToByteArray().Hash(HashType.Sha256);
        var fi = new FileInfo($"{salt.Encode(Codec.ByteHex)}.txt");
        var sut = fi.OpenCryptoWrite(salt, [], ".xyz");
        sut.Write([1, 2, 3, 6, 8, 99, 201]);

        // Act
        sut.FinaliseWrite();
        sut.Dispose();

        // Assert
        var span = File.ReadAllBytes(fi.FullName).AsSpan(32768, 2000).ToArray();
        span.Count(b => b == 0).Should().Be(0);
    }

    [Fact]
    public void Write_WhenFinalised_ProducesExpected()
    {
        // Arrange
        var testRef = Guid.NewGuid();
        var controlFi = new FileInfo($"{testRef}_write_control.avi");
        var testFiSrc = new FileInfo($"{testRef}_write_test_src.avi");
        var testFiTrg = new FileInfo($"{testRef}_write_test_trg.avi");
        File.Copy("TestObjects/sample.avi", controlFi.FullName);
        File.Copy("TestObjects/sample.avi", testFiSrc.FullName);
        var controlMd5 = controlFi.Hash(HashType.Md5).Encode(Codec.ByteHex);
        var salt = controlFi.EncryptInSitu(TestRefs.TestKey).Decode(Codec.ByteHex);
        controlFi.Delete();
        var sut = testFiTrg.OpenCryptoWrite(salt, TestRefs.TestKey, testFiSrc.Extension);
        var testSrc = testFiSrc.OpenRead();

        // Act
        testSrc.CopyTo(sut, sut.BufferLength);
        sut.CacheTrailer = true;
        sut.FinaliseWrite();
        sut.Dispose();
        testSrc.Dispose();
        var finalFi = testFiTrg.CopyTo(sut.Id);
        var clearFi = finalFi.DecryptHere(TestRefs.TestKey);
        var testMd5 = clearFi.Hash(HashType.Md5).Encode(Codec.ByteHex);

        // Assert
        sut.Metadata["filename"].Should().StartWith("_");
        testMd5.Should().Be(controlMd5);
        testFiSrc.Delete();
        testFiTrg.Delete();
        finalFi.Delete();
        clearFi.Delete();
    }

    [Fact]
    public void Read_WhenCalled_MatchesExpected()
    {
        // Arrange
        var testRef = Guid.NewGuid();
        var originalFi = new FileInfo($"{testRef}_read_original.avi");
        var testingFi = new FileInfo($"{testRef}_read_gcmtarget.avi");
        File.Copy("TestObjects/sample.avi", originalFi.FullName);
        var salt = RandomNumberGenerator.GetBytes(32);
        var originalFs = originalFi.OpenRead();
        var writer = testingFi.OpenCryptoWrite(salt, TestRefs.TestKey, originalFi.Extension);
        originalFs.CopyTo(writer);
        writer.CacheTrailer = true;
        writer.FinaliseWrite();
        var writerLen = writer.Length;
        writer.Dispose();
        originalFs.Dispose();
        var finalFi = testingFi.CopyTo(writer.Id);

        // Act
        var sut = finalFi.OpenCryptoRead(TestRefs.TestKey);
        var readerLen = sut.Length;
        var ctl = originalFi.OpenRead();
        var buffer = new byte[32768];

        var sutHex1 = Md5Hex(sut, buffer, 0, buffer.Length);
        var ctlHex1 = Md5Hex(ctl, buffer, 0, buffer.Length);

        // Assert
        writerLen.Should().Be(4000000);
        readerLen.Should().Be(3384888);
        sutHex1.Should().Be(ctlHex1);
        sut.Dispose();
        ctl.Dispose();
        originalFi.Delete();
        testingFi.Delete();
        finalFi.Delete();
    }

    [Fact]
    public void Read_StraddlingBlock_HashesExpected()
    {
        // Arrange
        const string name = "e092d33a6b0fa2abc987cba27f2da80fd07a1c6e3b7fe56a4ac53c486626c941.3faaccce33";
        var testRef = Guid.NewGuid().ToString();
        Directory.CreateDirectory(testRef);
        var cryptFi = new FileInfo($"{testRef}/{name}");
        File.Copy($"TestObjects/{name}", cryptFi.FullName);
        var sutStream = cryptFi.OpenCryptoRead(TestRefs.TestKey);
        var buffer = new byte[sutStream.BufferLength];

        // Act
        var blocksHash1 = Md5Hex(sutStream, buffer, 64000, 2000);

        // Assert
        blocksHash1.Should().Be("d41d8cd98f00b204e9800998ecf8427e");

        // Clean up
        sutStream.Dispose();
        cryptFi.Delete();
    }

    [Fact]
    public void Read_FinalBlock_HashesExpected()
    {
        // Arrange
        const string name = "e092d33a6b0fa2abc987cba27f2da80fd07a1c6e3b7fe56a4ac53c486626c941.3faaccce33";
        var testRef = Guid.NewGuid().ToString();
        Directory.CreateDirectory(testRef);
        var cryptFi = new FileInfo($"{testRef}/{name}");
        File.Copy($"TestObjects/{name}", cryptFi.FullName);
        var sutStream = cryptFi.OpenCryptoRead(TestRefs.TestKey);
        var buffer = new byte[sutStream.BufferLength];

        // Act
        var blocksHash1 = Md5Hex(sutStream, buffer, 8000, 733);

        // Assert
        blocksHash1.Should().Be("6dd3eeddd0bd6d9d5751cfc98b7da306");

        // Clean up
        sutStream.Dispose();
        cryptFi.Delete();
    }

    private static string Md5Hex(Stream stream, byte[] buffer, long position, int count)
    {
        Array.Clear(buffer, 0, buffer.Length);
        stream.Seek(position, SeekOrigin.Begin);
        var read = stream.Read(buffer, 0, count);
        return buffer.AsSpan(0, read).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);
    }
}
