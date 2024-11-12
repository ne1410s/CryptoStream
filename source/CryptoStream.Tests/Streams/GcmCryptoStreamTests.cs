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

    private static string Md5Hex(Stream stream, byte[] buffer, long position, int count)
    {
        Array.Clear(buffer, 0, buffer.Length);
        stream.Seek(position, SeekOrigin.Begin);
        var read = stream.Read(buffer, 0, count);
        return buffer.AsSpan(0, read).ToArray().Hash(HashType.Md5).Encode(Codec.ByteHex);
    }
}
